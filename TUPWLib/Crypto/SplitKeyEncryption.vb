'
' SPDX-FileCopyrightText: 2020 DB Systel GmbH
'
' SPDX-License-Identifier: Apache-2.0
'
' Licensed under the Apache License, Version 2.0 (the "License")
' You may not use this file except in compliance with the License.
'
' You may obtain a copy of the License at
'
'     http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software
' distributed under the License is distributed on an "AS IS" BASIS,
' WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
' See the License for the specific language governing permissions and
' limitations under the License.
'
' Author: Frank Schwab, DB Systel GmbH
'
' Version: 1.3.0
'
' Change history:
'    2020-05-05: V1.0.0: Created.
'    2020-05-14: V1.0.1: Removed unnecessary counting of source bytes.
'    2020-05-15: V1.1.0: Added exception comments and removed an unnecessary exception.
'    2020-05-26: V1.2.0: Make sure sensitive data is cleared from memory on encryption.
'    2020-05-29: V1.2.1: Refactored "RawDataEncryption" so it is more readable and structured.
'    2020-06-05: V1.2.2: Refactored "RawDataDecryption" to reliably clear sensitive data structures.
'    2020-06-18: V1.2.3: Removed unnecessary parentheses.
'    2020-08-11: V1.2.4: Refactored encryption to get rid of magic constant.
'    2020-08-11: V1.2.5: Corrected string concatenation and use array literal.
'    2020-11-12: V1.3.0: Implemented V6 of the encoded format.
'

Imports System.IO
Imports System.Security.Cryptography
Imports System.Text

''' <summary>
''' SplitKeyEncryption
''' </summary>
Public Class SplitKeyEncryption : Implements IDisposable
#Region "Private constants"
   ''' <summary>
   ''' Empty subject.
   ''' </summary>
   Private Const NO_SUBJECT As String = ""

   '
   ' Format id constants
   '
   Private Const FORMAT_ID_MIN As Byte = 1US
   Private Const FORMAT_ID_MAX As Byte = 6US

   Private Const FORMAT_ID_USE_BLINDING As Byte = 3US
   Private Const FORMAT_ID_USE_CORRECT_HMAC_KEY As Byte = 5US
   Private Const FORMAT_ID_USE_SAFE_ENCODING As Byte = 6US

   ''' <summary>
   ''' Separator for encryption string.
   ''' </summary>
   Private Const OLD_PARTS_SEPARATOR As Char = "$"c
   Private Const SAFE_PARTS_SEPARATOR As Char = "1"c

   ''' <summary>
   ''' Minimum length of key for HMAC algorithm.
   ''' </summary>
   Private Const MINIMUM_HMAC_KEY_LENGTH As Integer = 14

   ''' <summary>
   ''' Maximum length of key for HMAC algorithm.
   ''' </summary>
   ''' <remarks>
   ''' The HMAC key must not be larger than the block size of the underlying hash algorithm.
   ''' Here this is 32 bytes (256 bits). If the hash block size changes this constant
   ''' needs to be changed, as well.
   ''' </remarks>
   Private Const MAXIMUM_HMAC_KEY_LENGTH As Integer = 32

   ''' <summary>
   ''' Minimum length for source bytes.
   ''' </summary>
   Private Const MINIMUM_SOURCE_BYTES_LENGTH As UInteger = 100

   ''' <summary>
   ''' Maximum length fpr source bytes.
   ''' </summary>
   Private Const MAXIMUM_SOURCE_BYTES_LENGTH As UInteger = 10000000

   ''' <summary>
   ''' Minimum entropy for source bytes.
   ''' </summary>
   Private Const MINIMUM_SOURCE_BITS As UInteger = 128

   ''' <summary>
   ''' Minimum entropy per byte for source bytes.
   ''' </summary>
   Private Const ENTROPY_THRESHOLD As Double = 0.00012207   ' I.e. 1/2^13 which is representable as a simple floating point no.

   ''' <summary>
   ''' Salt prefix for key calculation.
   ''' </summary>
   Private Shared ReadOnly PREFIX_SALT As Byte() = {84, 117}   ' i.e "Tu"

   ''' <summary>
   ''' Salt postfix for key calculation.
   ''' </summary>
   Private Shared ReadOnly POSTFIX_SALT As Byte() = {112, 87}  ' i.e "pW"

   ''' <summary>
   ''' Character encoding (UTF-8) for conversion between characters and bytes.
   ''' </summary>
   Private Shared ReadOnly CHARACTER_ENCODING_FOR_DATA As Encoding = New UTF8Encoding(False, True) ' UTF-8 character encoding *with* error detection

#Region "Cipher mode constants"
   ' Format   Algorithm   Mode   Padding
   ' ------   ---------   ----   -------
   '    1     AES         CFB    Random
   '    2     AES         CTR    Random
   '    3     AES         CTR    Blinding
   '    4     AES         CBC    Blinding
   '    5     AES         CBC    Blinding
   '    6     AES         CBC    Blinding
   Private Const COUNTER_CIPHER_MODE As Byte = &HAAUS
   Private Const UNSUPPORTED_CIPHER_MODE As Byte = &HFFUS
   Private Shared ReadOnly CIPHERMODES_FOR_FORMAT_ID As Byte() = {
      UNSUPPORTED_CIPHER_MODE,
      CByte(CipherMode.CFB),
      COUNTER_CIPHER_MODE,
      COUNTER_CIPHER_MODE,
      CByte(CipherMode.CBC),
      CByte(CipherMode.CBC),
      CByte(CipherMode.CBC)
   }
#End Region
#End Region

#Region "Private class"
   ''' <summary>
   ''' Class to hold all the encryption information and handle them securely.
   ''' </summary>
   Private Class EncryptionParts
      '
      ' All data an encrypted string is comprised of.
      '
      Public formatId As Byte()
      Public iv As Byte()
      Public encryptedData As Byte()
      Public checksum As Byte()

      Public Sub New()
         formatId = New Byte(0 To 0) {0}
      End Sub

      ''' <summary>
      ''' Get length of all data in this instance.
      ''' </summary>
      ''' <returns>Total length of all data.</returns>
      Public ReadOnly Property TotalLength() As Integer
         Get
            Dim result As Integer = formatId.Length

            If iv IsNot Nothing Then _
               result += iv.Length

            If encryptedData IsNot Nothing Then _
               result += encryptedData.Length

            If checksum IsNot Nothing Then _
               result += checksum.Length

            Return result
         End Get
      End Property

      ''' <summary>
      ''' Clear all data.
      ''' </summary>
      Public Sub Zap()
         formatId(0) = 0

         If iv IsNot Nothing Then _
            ArrayHelper.Clear(iv)

         If encryptedData IsNot Nothing Then _
            ArrayHelper.Clear(encryptedData)

         If checksum IsNot Nothing Then _
            ArrayHelper.Clear(checksum)
      End Sub
   End Class
#End Region

#Region "Instance variables"
   '******************************************************************
   ' Instance variables
   '******************************************************************

   ''' <summary>
   ''' Encryption key to use.
   ''' </summary>
   Private m_EncryptionKey As ProtectedByteArray

   ''' <summary>
   ''' HMAC key to use.
   ''' </summary>
   Private m_HMACKey As ProtectedByteArray

   ''' <summary>
   ''' Indicates whether this instance has valid data.
   ''' </summary>
   Private m_IsValid As Boolean = False
#End Region

#Region "Constructors"
   '******************************************************************
   ' Constructor
   '******************************************************************

   ''' <summary>
   ''' Constructor for this instance.
   ''' </summary>
   ''' <remarks>
   ''' <b>Attention:</b> The caller is responsible for clearing the source byte arrays
   ''' with <c>ArrayHelper.Clear()</c> after they have been used here.
   ''' </remarks>
   ''' <param name="hmacKey">Key for the HMAC of the source bytes.</param>
   ''' <param name="sourceBytes">Source bytes that the key is derived from.</param>
   ''' <exception cref="ArgumentException">Thrown if the HMAC key is too short or too long or if there is not enough information
   ''' in the source bytes or there are too many source bytes.</exception>
   ''' <exception cref="ArgumentNullException">Thrown if the HMAC key or any of the source byte arrays is <c>Nothing</c>.</exception>
   Public Sub New(hmacKey As Byte(), ParamArray sourceBytes As Byte()())
#Disable Warning CA1062 ' Null check of "hmacKey" is done in CheckHMACKey
      CheckHMACKey(hmacKey)
#Enable Warning CA1062

      CheckSourceBytes(sourceBytes)

      SetKeysFromKeyAndSourceBytes(hmacKey, sourceBytes)

      m_IsValid = True
   End Sub
#End Region

#Region "Public methods"
   '******************************************************************
   ' Public methods
   '******************************************************************

#Region "Encryption interfaces"
   '
   ' Encryption interfaces
   '

   ''' <summary>
   ''' Encrypt a byte array under a subject.
   ''' </summary>
   ''' <param name="byteArrayToEncrypt">Byte array to encrypt.</param>
   ''' <param name="subject">The subject of this encryption.</param>
   ''' <returns>Printable form of the encrypted string.</returns>
   ''' <exception cref="ArgumentNullException">Thrown if any parameter is <c>Nothing</c>.</exception>
   Public Function EncryptData(byteArrayToEncrypt As Byte(), subject As String) As String
      RequireNonNull(byteArrayToEncrypt, NameOf(byteArrayToEncrypt))
      RequireNonNull(subject, NameOf(subject))

      Return MakeEncryptionStringFromSourceBytes(byteArrayToEncrypt, subject)
   End Function

   ''' <summary>
   ''' Encrypt a byte array.
   ''' </summary>
   ''' <param name="byteArrayToEncrypt">Byte array to encrypt.</param>
   ''' <returns>Printable form of the encrypted string.</returns>
   ''' <exception cref="ArgumentNullException">Thrown if <paramref name="byteArrayToEncrypt"/> is <c>Nothing</c>.</exception>
   Public Function EncryptData(byteArrayToEncrypt As Byte()) As String
      Return EncryptData(byteArrayToEncrypt, NO_SUBJECT)
   End Function

   ''' <summary>
   ''' Encrypt a character array under a subject.
   ''' </summary>
   ''' <param name="characterArrayToEncrypt">Character array to encrypt.</param>
   ''' <param name="subject">The subject of this encryption.</param>
   ''' <returns>Printable form of the encrypted string.</returns>
   ''' <exception cref="ArgumentNullException">Thrown if any parameter is <c>Nothing</c>.</exception>
   Public Function EncryptData(characterArrayToEncrypt As Char(), subject As String) As String
      RequireNonNull(characterArrayToEncrypt, NameOf(characterArrayToEncrypt))
      RequireNonNull(subject, NameOf(subject))

      Dim byteArrayToEncrypt As Byte() = CHARACTER_ENCODING_FOR_DATA.GetBytes(characterArrayToEncrypt)

      Dim result As String = MakeEncryptionStringFromSourceBytes(byteArrayToEncrypt, subject)

      ArrayHelper.Clear(byteArrayToEncrypt)

      Return result
   End Function

   ''' <summary>
   ''' Encrypt a character array.
   ''' </summary>
   ''' <param name="characterArrayToEncrypt">Character array to encrypt.</param>
   ''' <returns>Printable form of the encrypted string.</returns>
   ''' <exception cref="ArgumentNullException">Thrown if <paramref name="characterArrayToEncrypt"/> is <c>Nothing</c>.</exception>
   Public Function EncryptData(characterArrayToEncrypt As Char()) As String
      Return EncryptData(characterArrayToEncrypt, NO_SUBJECT)
   End Function

   ''' <summary>
   ''' Encrypt a string under a subject.
   ''' </summary>
   ''' <param name="stringToEncrypt">String to encrypt.</param>
   ''' <param name="subject">The subject of this encryption.</param>
   ''' <returns>Printable form of the encrypted string.</returns>
   ''' <exception cref="ArgumentNullException">Thrown if any parameter is <c>Nothing</c>.</exception>
   Public Function EncryptData(stringToEncrypt As String, subject As String) As String
      RequireNonNull(stringToEncrypt, NameOf(stringToEncrypt))
      RequireNonNull(subject, NameOf(subject))

      Dim byteArrayToEncrypt As Byte() = CHARACTER_ENCODING_FOR_DATA.GetBytes(stringToEncrypt)

      Dim result = MakeEncryptionStringFromSourceBytes(byteArrayToEncrypt, subject)

      ArrayHelper.Clear(byteArrayToEncrypt)

      Return result
   End Function

   ''' <summary>
   ''' Encrypt a string.
   ''' </summary>
   ''' <param name="stringToEncrypt">String to encrypt.</param>
   ''' <returns>Printable form of the encrypted string.</returns>
   ''' <exception cref="ArgumentNullException">Thrown if <paramref name="stringToEncrypt"/> is <c>Nothing</c>.</exception>
   Public Function EncryptData(stringToEncrypt As String) As String
      Return EncryptData(stringToEncrypt, NO_SUBJECT)
   End Function
#End Region

#Region "Decryption interfaces"
   '
   ' Decryption interfaces
   '

   ''' <summary>
   ''' Decrypt an encrypted string under a subject and return a byte array.
   ''' </summary>
   ''' <param name="stringToDecrypt">String to decrypt.</param>
   ''' <param name="subject">The subject of this decryption.</param>
   ''' <returns>Decrypted string as a byte array.</returns>
   ''' <exception cref="ArgumentException">Thrown if the <paramref name="stringToDecrypt"/> is incorrectly formatted or
   ''' contains invalid data.</exception>
   ''' <exception cref="ArgumentNullException">Thrown if any parameter is <c>Nothing</c>.</exception>
   ''' <exception cref="DataIntegrityException">Thrown if the calculated checksum does not match the checksum in the data.</exception>
   Public Function DecryptDataAsByteArray(stringToDecrypt As String, subject As String) As Byte()
      RequireNonNull(stringToDecrypt, NameOf(stringToDecrypt))
      RequireNonNull(subject, NameOf(subject))

      Dim subjectBytes As Byte() = CHARACTER_ENCODING_FOR_DATA.GetBytes(subject) ' Neither an ArgumentNullException, nor an EncoderFallbackException can happen.

      Dim result As Byte() = DecryptStringWithSubject(stringToDecrypt, subjectBytes)

      Return result
   End Function

   ''' <summary>
   ''' Decrypt an encrypted string and return a byte array.
   ''' </summary>
   ''' <param name="stringToDecrypt">String to decrypt.</param>
   ''' <returns>Decrypted string as a byte array.</returns>
   Public Function DecryptDataAsByteArray(stringToDecrypt As String) As Byte()
      Return DecryptDataAsByteArray(stringToDecrypt, NO_SUBJECT)
   End Function

   ''' <summary>
   ''' Decrypt an encrypted string under a subject and return a character array.
   ''' </summary>
   ''' <param name="stringToDecrypt">String to decrypt.</param>
   ''' <param name="subject">The subject of this decryption.</param>
   ''' <returns>Decrypted string as a character array.</returns>
   Public Function DecryptDataAsCharacterArray(stringToDecrypt As String, subject As String) As Char()
      Dim decryptedContent As Byte() = DecryptDataAsByteArray(stringToDecrypt, subject)

      Dim result As Char() = CHARACTER_ENCODING_FOR_DATA.GetChars(decryptedContent)

      ArrayHelper.Clear(decryptedContent)

      Return result
   End Function

   ''' <summary>
   ''' Decrypt an encrypted string and return a character array.
   ''' </summary>
   ''' <param name="stringToDecrypt">String to decrypt.</param>
   ''' <returns>Decrypted string as a character array.</returns>
   Public Function DecryptDataAsCharacterArray(stringToDecrypt As String) As Char()
      Return DecryptDataAsCharacterArray(stringToDecrypt, NO_SUBJECT)
   End Function

   ''' <summary>
   ''' Decrypt an encrypted string under a subject and return a string.
   ''' </summary>
   ''' <param name="stringToDecrypt">String to decrypt.</param>
   ''' <param name="subject">The subject of this decryption.</param>
   ''' <returns>Decrypted string as a string.</returns>
   Public Function DecryptDataAsString(stringToDecrypt As String, subject As String) As String
      Dim decryptedContent As Byte() = DecryptDataAsByteArray(stringToDecrypt, subject)

      Dim result As String = CHARACTER_ENCODING_FOR_DATA.GetString(decryptedContent)

      ArrayHelper.Clear(decryptedContent)

      Return result
   End Function

   ''' <summary>
   ''' Decrypt an encrypted string and return a string.
   ''' </summary>
   ''' <param name="stringToDecrypt">String to decrypt.</param>
   ''' <returns>Decrypted string as a string.</returns>
   Public Function DecryptDataAsString(stringToDecrypt As String) As String
      Return DecryptDataAsString(stringToDecrypt, NO_SUBJECT)
   End Function

#End Region

#Region "Validity interfaces"
   Public Function IsValid() As Boolean
      Return m_IsValid
   End Function
#End Region
#End Region

#Region "Private methods"
   '
   ' Private methods
   '

#Region "Check methods"
   '
   ' Check methods
   '
   ''' <summary>
   ''' Checkt the length of the HMAC key.
   ''' </summary>
   ''' <param name="aHMACKey">HMAC key.</param>
   ''' <exception cref="ArgumentException">Thrown, if the HMAC key is too short or too long.</exception>
   ''' <exception cref="ArgumentNullException">Thrown, if the HMAC key is <c>Nothing</c>.</exception>
   Private Shared Sub CheckHMACKey(aHMACKey As Byte())
      RequireNonNull(aHMACKey, NameOf(aHMACKey))

      If aHMACKey.Length < MINIMUM_HMAC_KEY_LENGTH Then _
         Throw New ArgumentException("HMAC key length is less than " & MINIMUM_HMAC_KEY_LENGTH.ToString())

      If aHMACKey.Length > MAXIMUM_HMAC_KEY_LENGTH Then _
         Throw New ArgumentException("HMAC key length is larger than " & MAXIMUM_HMAC_KEY_LENGTH.ToString())
   End Sub

   ''' <summary>
   ''' Check length of supplied source bytes.
   ''' </summary>
   ''' <param name="sourceBytes">Array of source byte arrays.</param>
   ''' <exception cref="ArgumentException">Thrown if there is not enough information in the source bytes or there are too many source bytes.</exception>
   ''' <exception cref="ArgumentNullException">Thrown if any of the source byte arrays is <c>Nothing</c>.</exception>
   Private Shared Sub CheckSourceBytes(ParamArray sourceBytes As Byte()())
      Dim ec As EntropyCalculator = New EntropyCalculator()

      Dim sourceLength As Integer

      For i As Integer = 0 To sourceBytes.Length - 1
         RequireNonNull(sourceBytes(i), NameOf(sourceBytes) & "(" & i.ToString() & ")")

         sourceLength = sourceBytes(i).Length

         If sourceLength > 0 Then
            ec.AddBytes(sourceBytes(i))
         Else
            Throw New ArgumentException(NameOf(sourceBytes) & "(" & i.ToString() & ") has 0 length")
         End If
      Next

      If ec.GetInformationInBits() < MINIMUM_SOURCE_BITS Then
         Dim entropy As Double = ec.GetEntropy()

         If entropy > ENTROPY_THRESHOLD Then
            Throw New ArgumentException("There is not enough information provided in the source bytes. Try to increase the length to at least " & CInt(Math.Round(MINIMUM_SOURCE_BITS / entropy) + 1).ToString() & " bytes")
         Else
            Throw New ArgumentException("There is no information provided in the source bytes (i.e. there are only identical byte values). Use bytes with varying values.")
         End If
      End If

      If ec.Count < MINIMUM_SOURCE_BYTES_LENGTH Then _
         Throw New ArgumentException("There are less than " & MINIMUM_SOURCE_BYTES_LENGTH.ToString() & " source bytes")

      If ec.Count > MAXIMUM_SOURCE_BYTES_LENGTH Then _
         Throw New ArgumentException("There are more than " & MAXIMUM_SOURCE_BYTES_LENGTH.ToString() & " source bytes")
   End Sub
#End Region

#Region "Constructor helpers"
   ''' <summary>
   ''' Set the keys of this instance from the supplied byte arrays and a HMAC key.
   ''' </summary>
   ''' <param name="hmacKey">HMAC key to be used for key calculation.</param>
   ''' <param name="sourceBytes">Source bytes to be used for key calculation.</param>
   Private Sub SetKeysFromKeyAndSourceBytes(hmacKey As Byte(), ParamArray sourceBytes As Byte()())
      Dim hmacOfSourceBytes As Byte() = GetHMACValueOfSourceBytes(hmacKey, sourceBytes)

      ' 1. half of file HMAC is used as the encryption key of this instance
      Dim keyPart As Byte() = ArrayHelper.CopyOf(hmacOfSourceBytes, 0, 16)

      m_EncryptionKey = New ProtectedByteArray(keyPart)

      ArrayHelper.Clear(keyPart)

      ' 2. half of file HMAC is used as the HMAC key of this instance
      keyPart = ArrayHelper.CopyOf(hmacOfSourceBytes, 16, 16)

      ArrayHelper.Clear(hmacOfSourceBytes)

      m_HMACKey = New ProtectedByteArray(keyPart)
      ArrayHelper.Clear(keyPart)
   End Sub

   ''' <summary>
   ''' Calculate the HMAC of the source bytes.
   ''' </summary>
   ''' <remarks>
   ''' This is the source for the encryption and the HMAC keys used in encryption process. 
   ''' </remarks>
   ''' <param name="key">HMAC key.</param>
   ''' <param name="sourceBytes">Source bytes used in HMAC calculation.</param>
   ''' <returns>HMAC value of the source bytes.</returns>
   Private Shared Function GetHMACValueOfSourceBytes(key As Byte(), ParamArray sourceBytes As Byte()()) As Byte()
      Dim result As Byte()

      Using byteStream As MemoryStream = New MemoryStream
         Using hmac As New HMACSHA256(key)
            Using hmacStream As CryptoStream = New CryptoStream(byteStream, hmac, CryptoStreamMode.Write)
               For i As Integer = 0 To sourceBytes.Length - 1
                  hmacStream.Write(sourceBytes(i), 0, sourceBytes(i).Length)
               Next

               hmacStream.FlushFinalBlock()
            End Using

            result = hmac.Hash
         End Using
      End Using

      Return result
   End Function
#End Region

#Region "Encryption implementation"
   ''' <summary>
   ''' Encrypt the source bytes and return the encryption string.
   ''' </summary>
   ''' <param name="sourceBytes">Source bytes to encrypt.</param>
   ''' <param name="subject">Subject to use for encryption.</param>
   ''' <returns>Formatted string with the encrypted data.</returns>
   Private Function MakeEncryptionStringFromSourceBytes(sourceBytes As Byte(), subject As String) As String
      Dim result As String

      Dim rawEncryptionData As EncryptionParts = Nothing  ' This needs to be declared here so it is accessible in the catch block
      Dim subjectBytes As Byte() = CHARACTER_ENCODING_FOR_DATA.GetBytes(subject)

      Try
         rawEncryptionData = RawDataEncryption(sourceBytes, subjectBytes)

         result = MakeEncryptionStringFromEncryptionParts(rawEncryptionData)

         rawEncryptionData.Zap()

         '      Catch ex As Exception
         '        Throw New InvalidCryptoParameterException("Invalid cryptographic parameter: " + ex.ToString(), ex)

      Catch ex As Exception
         If rawEncryptionData IsNot Nothing Then _
            rawEncryptionData.Zap()

         Throw   ' Rethrow exception
      End Try

      Return result
   End Function

   ''' <summary>
   ''' Encrypt the source bytes and put the result in the <see cref="EncryptionParts"/> class.
   ''' </summary>
   ''' <param name="sourceBytes">Source bytes to encrypt.</param>
   ''' <param name="subjectBytes">Subject bytes to use for encryption.</param>
   ''' <returns>An <see cref="EncryptionParts"/> object with the encrypted data.</returns>
   Private Function RawDataEncryption(sourceBytes As Byte(), subjectBytes As Byte()) As EncryptionParts
      Dim result As EncryptionParts
      Dim encryptionKey As Byte() = Nothing

      '
      ' There is no "finally" statement as this will only be executed if there is a wrapping "try-catch" block.
      ' To ensure that the array is cleared the corresponding statements are duplicated in the "try"
      ' and the "catch" part.
      '
      Try
         encryptionKey = GetKeyForEncryptionDependingOnSubject(subjectBytes)

         result = GetEncryptionPartsForBytesAndKey(sourceBytes, encryptionKey)

         '
         ' Clear sensitive data
         '
         ArrayHelper.Clear(encryptionKey)

      Catch ex As Exception
         '
         ' Clear sensitive data
         '
         If encryptionKey IsNot Nothing Then _
            ArrayHelper.Clear(encryptionKey)

         Throw
      End Try

      result.checksum = GetChecksumForEncryptionParts(result, subjectBytes)

      Return result
   End Function

   ''' <summary>
   ''' Get a secure initialization vector.
   ''' </summary>
   ''' <param name="blockSizeInBytes"></param>
   ''' <returns>Initialization vector with a size of <paramref name="blockSizeInBytes"/>.</returns>
   Private Shared Function GetInitializationVector(blockSizeInBytes As Integer) As Byte()
      Dim result As Byte() = New Byte(0 To blockSizeInBytes - 1) {}

      SecurePseudoRandomNumberGenerator.GetBytes(result)

      Return result
   End Function

   ''' <summary>
   ''' Get <see cref="EncryptionParts"/> object for supplied data and key.
   ''' </summary>
   ''' <param name="sourceBytes">Source bytes to encrypt.</param>
   ''' <param name="key">Key to use for encryption.</param>
   ''' <returns><see cref="EncryptionParts"/> object with all parts of the encryption of the <paramref name="sourceBytes"/>.</returns>
   Private Shared Function GetEncryptionPartsForBytesAndKey(sourceBytes As Byte(), key() As Byte) As EncryptionParts
      Dim result As New EncryptionParts

      result.formatId(0) = FORMAT_ID_MAX

      Dim paddedSourceBytes As Byte() = Nothing

      Dim aesCipher As AesCng = New AesCng() With {
         .Mode = CipherMode.CBC,
         .Padding = PaddingMode.None   ' Never use any of the standard paddings!!!! They are all susceptible to a padding oracle.
         }

      Dim blockSizeInBytes As Integer = aesCipher.BlockSize >> 3

      result.iv = GetInitializationVector(blockSizeInBytes)

      '
      ' This is a "try-catch" block instead of a "using" construct because we need to clear the contents of
      ' an array before handling the exception. A "using" construct would only dispose of the "aesCipher".
      '
      ' There is no "finally" statement as this will only be executed if there is a wrapping "try-catch" block.
      ' To ensure that the array is cleared the corresponding statements are duplicated in the "try"
      ' and the "catch" part.
      '
      Try
         paddedSourceBytes = PadSourceBytes(sourceBytes, blockSizeInBytes)

         ' Encrypt the source string with the iv
         result.encryptedData = GetEncryptedBytes(aesCipher, paddedSourceBytes, key, result.iv)

         ArrayHelper.Clear(paddedSourceBytes)   ' Clear sensitive data

         aesCipher.Dispose()

      Catch ex As Exception
         If paddedSourceBytes IsNot Nothing Then _
            ArrayHelper.Clear(paddedSourceBytes)   ' Clear sensitive data

         aesCipher.Dispose()

         Throw
      End Try

      Return result
   End Function

   ''' <summary>
   ''' Encrypt a byte array under a cipher, a key and an iv.
   ''' </summary>
   ''' <param name="aSymmetricCipher">The cipher under which to encrypt.</param>
   ''' <param name="sourceBytes">The bytes to be encrypted.</param>
   ''' <param name="key">The key to use for encryption.</param>
   ''' <param name="iv">The initialization vector to use for encryption.</param>
   ''' <returns>Encrypted bytes.</returns>
   Private Shared Function GetEncryptedBytes(aSymmetricCipher As SymmetricAlgorithm, sourceBytes As Byte(), key As Byte(), iv As Byte()) As Byte()
      Dim result As Byte()

      Using symmetricEncryptor As ICryptoTransform = aSymmetricCipher.CreateEncryptor(key, iv)
         Using resultStream As New MemoryStream()
            Using encryptionStream As New CryptoStream(resultStream, symmetricEncryptor, CryptoStreamMode.Write)
               encryptionStream.Write(sourceBytes, 0, sourceBytes.Length)

               encryptionStream.FlushFinalBlock()
            End Using

            result = resultStream.ToArray()
         End Using
      End Using

      Return result
   End Function

   ''' <summary>
   ''' Blind and pad source bytes.
   ''' </summary>
   ''' <param name="sourceBytes">The source bytes to process.</param>
   ''' <param name="blockSizeInBytes">The block size to be used for blinding and padding.</param>
   ''' <returns>Byte array with blinded and padded source bytes.</returns>
   Private Shared Function PadSourceBytes(sourceBytes() As Byte, blockSizeInBytes As Integer) As Byte()
      Dim result As Byte()

      Dim blindedSourceBytes As Byte() = Nothing

      '
      ' Do not use "finally" as this is only executed if there is a wrapping try-catch-block
      '
      Try
         ' Ensure that blinded array needs at least 2 AES blocks, so the length of the encrypted data
         ' can not be inferred to be no longer than block size - 3 bytes (= 13 bytes for AES).
         blindedSourceBytes = ByteArrayBlinding.BuildBlindedByteArray(sourceBytes, blockSizeInBytes + 1)

         result = RandomPadding.AddPadding(blindedSourceBytes, blockSizeInBytes)

         ArrayHelper.Clear(blindedSourceBytes)  ' Clear sensitive data

      Catch ex As Exception
         If blindedSourceBytes IsNot Nothing Then _
            ArrayHelper.Clear(blindedSourceBytes)  ' Clear sensitive data

         Throw   ' Rethrow exception
      End Try

      Return result
   End Function

   ''' <summary>
   ''' Get the encryption key depending on whether there is a subject present, or not.
   ''' </summary>
   ''' <param name="subjectBytes">The bytes of the subject.</param>
   ''' <returns>Encryption key depending on the <paramref name="subjectBytes"/>.</returns>
   Private Function GetKeyForEncryptionDependingOnSubject(subjectBytes As Byte()) As Byte()
      If subjectBytes.Length > 0 Then
         Return GetKeyForSubjectWithHMACKey(m_HMACKey, m_EncryptionKey, subjectBytes)
      Else
         Return GetDefaultKeyForEncryption()
      End If
   End Function

   ''' <summary>
   ''' Get the default encryption key.
   ''' </summary>
   ''' <returns>The default encyrption key.</returns>
   Private Function GetDefaultKeyForEncryption() As Byte()
      Return m_EncryptionKey.GetData()
   End Function
#End Region

#Region "Decryption implementation"
   ''' <summary>
   ''' Decrypt an encryption string with a subject.
   ''' </summary>
   ''' <param name="stringToDecrypt">Encryption string to decrypt.</param>
   ''' <param name="subjectBytes">Subject bytes to use for decryption.</param>
   ''' <returns>Decrypted bytes.</returns>
   ''' <exception cref="ArgumentException">Thrown if no. of parts in string is not correct or the 
   ''' format id is unknown or invalid or if the size of the iv in <paramref name="stringToDecrypt"/> is not the same as 
   ''' the block size of the used algorithm or if any part of the string is not a valid Base64 string.</exception>
   ''' <exception cref="DataIntegrityException">Thrown if the calculated checksum does not match the checksum in the data.</exception>
   Private Function DecryptStringWithSubject(stringToDecrypt As String, subjectBytes() As Byte) As Byte()
      Dim result As Byte()

      Dim ep As EncryptionParts = Nothing

      Try
         ep = GetPartsFromPrintableString(stringToDecrypt)

         CheckChecksumForEncryptionParts(ep, subjectBytes)

         result = RawDataDecryption(ep, subjectBytes)

         ep.Zap()

      Catch ex As FormatException
         ' Ensure sensitive data are cleared
         If ep IsNot Nothing Then _
            ep.Zap()

         Throw New ArgumentException("Invalid Base64 encoding", ex)

      Catch ex As Exception
         ' Ensure sensitive data are cleared
         If ep IsNot Nothing Then _
            ep.Zap()

         Throw   ' Rethrow exception
      End Try

      Return result
   End Function

   ''' <summary>
   ''' Check the checksum of the encrypted parts that have been read.
   ''' </summary>
   ''' <param name="ep">Encryption parts to be checked.</param>
   ''' <param name="subjectBytes">Subject bytes to use for HMAC key calculation.</param>
   ''' <exception cref="DataIntegrityException">Thrown if the calculated checksum does not match the checksum in the data.</exception>
   Private Sub CheckChecksumForEncryptionParts(ep As EncryptionParts, subjectBytes As Byte())
      Dim calculatedChecksum As Byte() = GetChecksumForEncryptionParts(ep, subjectBytes)

      If Not ArrayHelper.SecureAreEqual(calculatedChecksum, ep.checksum) Then _
         Throw New DataIntegrityException("Checksum does not match data")
   End Sub

   ''' <summary>
   ''' Return unpadded string bytes depending on format id.
   ''' </summary>
   ''' <param name="formatId">Format id of data.</param>
   ''' <param name="paddedDecryptedStringBytes">Byte array of padded decrypted bytes.</param>
   ''' <returns>Unpadded decrypted bytes.</returns>
   Private Shared Function GetUnpaddedStringBytes(formatId As Byte, paddedDecryptedStringBytes As Byte()) As Byte()
      ' Formats 1 and 2 use padding. Starting from format 3 blinding is used.
      If formatId >= FORMAT_ID_USE_BLINDING Then
         Return ByteArrayBlinding.UnBlindByteArray(paddedDecryptedStringBytes)
      Else
         Return ArbitraryTailPadding.RemovePadding(paddedDecryptedStringBytes)
      End If
   End Function

   ''' <summary>
   ''' Decrypt data that have been created by the corresponding encryption.
   ''' </summary>
   ''' <param name="ep">The encryption parts of the data.</param>
   ''' <param name="subjectBytes">The subject for this decryption.</param>
   ''' <returns>Decrypted data as a byte array.</returns>
   ''' <exception cref="ArgumentException">Thrown if the size of the iv in <paramref name="ep"/> is not the same as 
   ''' the block size of the used algorithm.</exception>
   Private Function RawDataDecryption(ep As EncryptionParts, subjectBytes As Byte()) As Byte()
      Dim result As Byte()

      ' "ep.formatId" has been checked in "decryptData" and does not need to be checked here
      Dim cipherModeForFormatId As Byte = CIPHERMODES_FOR_FORMAT_ID(ep.formatId(0))

      Dim decryptionKey As Byte() = Nothing

      Dim paddedDecryptedStringBytes As Byte() = Nothing

      '
      ' Wrap everything in a try-catch-block as there are data that need to be cleared before rethrowing
      ' an exception. The finally part is not used as it is only executed if there is a wrapping 
      ' try-catch-block. So the clearing of the sensitive data is repeated in both parts of the try-catch-block.
      '
      Try
         decryptionKey = GetKeyForEncryptionDependingOnSubject(subjectBytes)

         Using aesDecryptor = GetCryptoTransformForCipherMode(cipherModeForFormatId, decryptionKey, ep.iv)
            paddedDecryptedStringBytes = DecryptDataWithCryptoTransform(aesDecryptor, ep.encryptedData)
         End Using

         ArrayHelper.Clear(decryptionKey)   ' Clear sensitive data

         result = GetUnpaddedStringBytes(ep.formatId(0), paddedDecryptedStringBytes)

         ArrayHelper.Clear(paddedDecryptedStringBytes)   ' Clear sensitive data

      Catch ex As Exception
         '
         ' Clear sensitive data
         '
         If decryptionKey IsNot Nothing Then _
            ArrayHelper.Clear(decryptionKey)

         If paddedDecryptedStringBytes IsNot Nothing Then _
            ArrayHelper.Clear(paddedDecryptedStringBytes)

         Throw   ' Rethrow exception
      End Try

      Return result
   End Function

   ''' <summary>
   ''' Get a crypto transform depending on the ciphermode.
   ''' </summary>
   ''' <param name="aCipherMode">The cipher mode.</param>
   ''' <param name="key">Key for the crypto transform.</param>
   ''' <param name="iv">Iv for the crypto transform.</param>
   ''' <returns>Crypto transform for the cipher mode.</returns>
   ''' <exception cref="ArgumentException">Thrown if the size of the <paramref name="iv"/> is not the same as 
   ''' the block size of the used algorithm.</exception>
   Private Shared Function GetCryptoTransformForCipherMode(aCipherMode As Byte, key As Byte(), iv As Byte()) As ICryptoTransform
      Dim result As ICryptoTransform

      Using aesAlgorithm As SymmetricAlgorithm = New AesCng()
         If aCipherMode <> COUNTER_CIPHER_MODE Then
            aesAlgorithm.Mode = CType(aCipherMode, CipherMode)
            aesAlgorithm.Padding = PaddingMode.None

            result = aesAlgorithm.CreateDecryptor(key, iv)
         Else
            ' .Net does not support counter mode natively, so we have to use our own.
            result = New CounterModeCryptoTransform(aesAlgorithm, key, iv)
         End If
      End Using

      Return result
   End Function

   ''' <summary>
   ''' Decrypt data with a specific crypto transform.
   ''' </summary>
   ''' <param name="ct">The crypto transform to use.</param>
   ''' <param name="encryptedData">Data to decrypt.</param>
   ''' <returns>Decrypted data.</returns>
   ''' <exception cref="CryptographicException">Thrown if the key is corrupt.</exception>
   Private Shared Function DecryptDataWithCryptoTransform(ct As ICryptoTransform, encryptedData As Byte()) As Byte()
      Dim result As Byte()

      Using resultStream As New MemoryStream()
         Using encryptionStream As New CryptoStream(resultStream, ct, CryptoStreamMode.Write)
            encryptionStream.Write(encryptedData, 0, encryptedData.Length)

            encryptionStream.FlushFinalBlock()
         End Using

         result = resultStream.ToArray()
      End Using

      Return result
   End Function
#End Region

#Region "String builder and separator methods for encrypted string"
   ''' <summary>
   ''' Make encryption string from the different parts of the encryption.
   ''' </summary>
   ''' <param name="ep">Encryption parts that are to be converted to a string.</param>
   ''' <returns>Encryption string.</returns>
   Private Shared Function MakeEncryptionStringFromEncryptionParts(ep As EncryptionParts) As String
      Dim sb As New StringBuilder(CalculateStringBuilderCapacityForEncryptionParts(ep))

      sb.Append(ep.formatId(0))
      sb.Append(SAFE_PARTS_SEPARATOR)
      sb.Append(Base32Encoding.EncodeSpellSafeNoPadding(ep.iv))
      sb.Append(SAFE_PARTS_SEPARATOR)
      sb.Append(Base32Encoding.EncodeSpellSafeNoPadding(ep.encryptedData))
      sb.Append(SAFE_PARTS_SEPARATOR)
      sb.Append(Base32Encoding.EncodeSpellSafeNoPadding(ep.checksum))

      Return sb.ToString()
   End Function

   ''' <summary>
   ''' Calculate capacity of StringBuilder for encryption parts.
   ''' </summary>
   ''' <remarks>The size of the final string is 4 + SumOf(ceil(ArrayLength * 8 / 5)).
   ''' This is a complicated expression which is overestimated by the easier
   ''' expression 4 + SumOfArrayLengths * 7 / 4.
   ''' </remarks>
   ''' <param name="ep">Encryption parts to calculate the capacity for</param>
   ''' <returns>Slightly overestimated capacity of the StringBuilder for the
   ''' supplied encryption parts.</returns>
   Private Shared Function CalculateStringBuilderCapacityForEncryptionParts(ep As EncryptionParts) As Integer
      Dim arrayLengths As Integer = ep.TotalLength

      Return 4 + arrayLengths + (arrayLengths >> 1) + (arrayLengths >> 2)
   End Function

   ''' <summary>
   ''' Convert an encryption string into its parts.
   ''' </summary>
   ''' <param name="encryptionText">Encryption string to be decrypted</param>
   ''' <returns>Encryption parameters as an <see cref="EncryptionParts"/> object.</returns>
   ''' <exception cref="ArgumentException">Thrown if no. of parts in string is not correct or the 
   ''' format id is unknown or invalid.</exception>
   ''' <exception cref="FormatException">Thrown if any part of the string is not a valid Base64 string.</exception>
   Private Shared Function GetPartsFromPrintableString(encryptionText As String) As EncryptionParts
      Dim formatCharacter As String = encryptionText(0)

      Dim formatId As Byte

      If Not Byte.TryParse(formatCharacter, formatId) Then _
         Throw New ArgumentException("Invalid format id", NameOf(encryptionText))

      If (formatId < FORMAT_ID_MIN) OrElse (formatId > FORMAT_ID_MAX) Then _
         Throw New ArgumentException("Unknown format id", NameOf(encryptionText))

      Dim separator As Char

      If formatId >= FORMAT_ID_USE_SAFE_ENCODING Then
         separator = SAFE_PARTS_SEPARATOR
      Else
         separator = OLD_PARTS_SEPARATOR
      End If

      Dim parts As String() = encryptionText.Split(separator)

      Dim result As EncryptionParts = New EncryptionParts()

      result.formatId(0) = formatId

      If parts.Length = 4 Then
         If formatId >= FORMAT_ID_USE_SAFE_ENCODING Then
            result.iv = Base32Encoding.DecodeSpellSafe(parts(1))
            result.encryptedData = Base32Encoding.DecodeSpellSafe(parts(2))
            result.checksum = Base32Encoding.DecodeSpellSafe(parts(3))
         Else
            result.iv = UnpaddedBase64.FromUnpaddedBase64String(parts(1))
            result.encryptedData = UnpaddedBase64.FromUnpaddedBase64String(parts(2))
            result.checksum = UnpaddedBase64.FromUnpaddedBase64String(parts(3))
         End If
      Else
         Throw New ArgumentException("Number of '" & separator & "' separated parts in encrypted text is not 4")
      End If

      Return result
   End Function
#End Region

#Region "Checksum methods"
   ''' <summary>
   ''' Calculate the HMAC of the encrypted parts.
   ''' </summary>
   ''' <param name="ep">Encrypted parts to calculate the checksum for.</param>
   ''' <param name="subjectBytes">The subject for this HMAC calculation.</param>
   ''' <returns>HMAC value calculated for <paramref name="ep"/> and <paramref name="ep"/>.</returns>
   Private Function GetChecksumForEncryptionParts(ep As EncryptionParts, subjectBytes As Byte()) As Byte()
      Dim result As Byte()

      Dim hmacKey As Byte() = GetHMACKeyForFormat(ep.formatId(0), subjectBytes)

      Using encryptionPartsStream As MemoryStream = New MemoryStream(ep.TotalLength)
         Using hmac As HMACSHA256 = New HMACSHA256(hmacKey)
            Using hmacStream As CryptoStream = New CryptoStream(encryptionPartsStream, hmac, CryptoStreamMode.Write)
               hmacStream.Write(ep.formatId, 0, ep.formatId.Length)
               hmacStream.Write(ep.iv, 0, ep.iv.Length)
               hmacStream.Write(ep.encryptedData, 0, ep.encryptedData.Length)

               hmacStream.FlushFinalBlock()
            End Using

            result = hmac.Hash
         End Using

         ArrayHelper.Clear(hmacKey)
      End Using

      Return result
   End Function

   ''' <summary>
   ''' Get the HMAC key depending on the format id.
   ''' </summary>
   ''' <remarks>
   ''' The HMAC key is correctly calculated from format id 5 on. Before that the wrong key
   ''' was used for HMAC calculation when used with a subject. If there was no subject the
   ''' HMAC key has always been correct.
   ''' </remarks>
   ''' <param name="formatId">Format id for which the HMAC key is to be calculated.</param>
   ''' <param name="subjectBytes">Subject bytes to be used for key calculation.</param>
   ''' <returns>HMAC key.</returns>
   Private Function GetHMACKeyForFormat(formatId As Byte, subjectBytes() As Byte) As Byte()
      Dim result As Byte()

      If formatId >= FORMAT_ID_USE_CORRECT_HMAC_KEY Then
         result = GetKeyForHMACDependingOnSubject(subjectBytes)
      Else
         result = GetDefaultKeyForHMAC()
      End If

      Return result
   End Function

   ''' <summary>
   '''  Get the HMAC key depending on whether there is a subject present, or not.
   ''' </summary>
   ''' <param name="subjectBytes">The bytes of the subject.</param>
   ''' <returns>HMAC key depending on the <paramref name="subjectBytes"/>.</returns>
   Private Function GetKeyForHMACDependingOnSubject(subjectBytes As Byte()) As Byte()
      If subjectBytes.Length > 0 Then
         Return GetKeyForSubjectWithHMACKey(m_EncryptionKey, m_HMACKey, subjectBytes)
      Else
         Return GetDefaultKeyForHMAC()
      End If
   End Function

   ''' <summary>
   ''' Get the default HMAC key.
   ''' </summary>
   ''' <returns>The default HMAC key.</returns>
   Private Function GetDefaultKeyForHMAC() As Byte()
      Return m_HMACKey.GetData()
   End Function
#End Region

#Region "Key calculation method"
   ''' <summary>
   ''' Caluclate a key with a HMAC key from a base key and a subject.
   ''' </summary>
   ''' <param name="hmacKey">HMAC key to be used for key calculation.</param>
   ''' <param name="baseKey">The base key from which the new key is derived.</param>
   ''' <param name="subjectBytes">The subject bytes that are used to modify the <paramref name="baseKey"/>.</param>
   ''' <returns>Key derived from the parameters.</returns>
   Private Shared Function GetKeyForSubjectWithHMACKey(hmacKey As ProtectedByteArray,
                                                       baseKey As ProtectedByteArray,
                                                       subjectBytes As Byte()) As Byte()
      Dim result As Byte()

      Using byteStream As MemoryStream = New MemoryStream()
         Dim hmacKeyBytes As Byte() = hmacKey.GetData()

         Using hmac As New HMACSHA256(hmacKeyBytes)
            Using hmacStream As CryptoStream = New CryptoStream(byteStream, hmac, CryptoStreamMode.Write)
               ' 1. Hash base key
               Dim baseKeyBytes As Byte() = baseKey.GetData()
               hmacStream.Write(baseKeyBytes, 0, baseKey.Length)
               ArrayHelper.Clear(baseKeyBytes)

               ' 2. Hash subject bytes fenced by the salt bytes
               hmacStream.Write(PREFIX_SALT, 0, PREFIX_SALT.Length)
               hmacStream.Write(subjectBytes, 0, subjectBytes.Length)
               hmacStream.Write(POSTFIX_SALT, 0, POSTFIX_SALT.Length)

               hmacStream.FlushFinalBlock()
            End Using

            result = hmac.Hash
         End Using

         ArrayHelper.Clear(hmacKeyBytes)
      End Using

      Return result
   End Function
#End Region

#Region "Exception helpers"
   ''' <summary>
   ''' Check if object is null and throw an exception, if it is.
   ''' </summary>
   ''' <param name="anObject">Object to check.</param>
   ''' <param name="parameterName">Parameter name for exception.</param>
   ''' <exception cref="ArgumentNullException">Thrown when <paramref name="anObject"/> is <c>Nothing</c>.</exception>
   Private Shared Sub RequireNonNull(anObject As Object, parameterName As String)
      If anObject Is Nothing Then _
         Throw New ArgumentNullException(parameterName)
   End Sub
#End Region
#End Region

#Region "IDisposable support"
   ''' <summary>
   ''' Marker, if disposition of managed resources has already been done.
   ''' </summary>
   Private m_IsDisposed As Boolean ' To detect redundant calls

   ''' <summary>
   ''' Dispose managed and unmanged resources.
   ''' </summary>
   ''' <param name="disposeManagedResources"><c>true</c>, if managed resource are to be disposed of, <c>false</c>, if not.</param>
   Protected Overridable Sub Dispose(disposeManagedResources As Boolean)
      If Not m_IsDisposed Then
         m_IsDisposed = True

         If disposeManagedResources Then
            If m_IsValid Then
               '
               ' Disposing of resources needs to be synchronized to prevent a race condition.
               '
               SyncLock m_EncryptionKey
                  m_IsValid = False

                  m_EncryptionKey.Dispose()
                  m_HMACKey.Dispose()
               End SyncLock
            End If
         End If

         ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
         ' TODO: set large fields to null.
      End If
   End Sub

   ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
   'Protected Overrides Sub Finalize()
   '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
   '    Dispose(False)
   '    MyBase.Finalize()
   'End Sub

   ''' <summary>
   ''' Dispose of resources.
   ''' </summary>
   ''' <remarks>
   ''' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
   ''' </remarks>
   Public Sub Dispose() Implements IDisposable.Dispose
      Dispose(True)
      ' TODO: uncomment the following line if Finalize() is overridden above.
      ' GC.SuppressFinalize(Me)
   End Sub
#End Region
End Class
