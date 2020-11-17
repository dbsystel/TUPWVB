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
' Version: 1.0.1
'
' Change history:
'    2020-05-05: V1.0.0: Created.
'    2020-10-27: V1.0.1: Added test case for empty HMAC.
'

Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports DB.BCM.TUPW

<TestClass()> Public Class SplitKeyEncryptionTest
#Region "Private constants"
   '
   ' Private constants
   '
   ''' <summary>
   ''' HMAC key to be used for encryption.
   ''' </summary>
   Private Shared ReadOnly CONSTANT_HMAC_KEY As Byte() = {
           &HC1, &HC2, &HC8, &HF,
           &HDE, &H75, &HD7, &HA9,
           &HFC, &H92, &H56, &HEA,
           &H3C, &HC, &H7A, &H8,
           &H8A, &H6E, &HB5, &H78,
           &H15, &H79, &HCF, &HB4,
           &H2, &HF, &H38, &H3C,
           &H61, &H4F, &H9D, &HDB}

   Private Shared ReadOnly COMPUTED_HMAC_KEY As Byte() = New Byte(0 To 31) {}

   ''' <summary>
   ''' Default character set for string to byte conversion.
   ''' </summary>
   Private Shared ReadOnly CHARACTER_ENCODING_FOR_DATA As Encoding = Encoding.UTF8

   '
   ' Error message constants
   '
   Private Const TEXT_DECRYPTION_MISMATCH_MESSAGE As String = "Decrypted text is not the same as original text"
   Private Const TEXT_EXPECTED_EXCEPTION As String = "Expected exception not thrown"
   Private Const TEXT_UNEXPECTED_EXCEPTION As String = "Unexpected exception: "
   Private Const TEXT_SEPARATOR As String = " / "


   '
   ' Some key elements.
   '
   ' For the sake of repeatable tests the source bytes are constants in this test file.
   ' In real use use one <b>must never</b> use values that originate <em>in</em> the program.
   ' All source <b>are required</b> to originate from <em>outside</em> the program!
   '
   Private Const SOURCE_TEXT_1 As String = "The quick brown fox jumped over the lazy dog"
   Private Const SOURCE_TEXT_2 As String = "314159265358979323846264338327952718281828459045235360287471352722459157718361045473427152204544"
   Private Const SOURCE_TEXT_3 As String = "The answer to the Ultimate Question of Life, the Universe, and Everything"
   Private Shared SOURCE_BYTES_1 As Byte()
   Private Shared SOURCE_BYTES_2 As Byte()
   Private Shared SOURCE_BYTES_3 As Byte()
   Private Shared ReadOnly SOURCE_BYTES_4 As Byte() = New Byte(0 To 1999) {}
   Private Shared ReadOnly NON_RANDOM_SOURCE_BYTES As Byte() = New Byte(0 To 99999) {}

   '
   ' Known clear text to encrypt
   '
   Private Const CLEAR_TEXT_V3 As String = "This is a clear Text"
   Private Const CLEAR_TEXT_V5 As String = "This#" & ChrW(&H201D) & "s?a§StR4nGé€PàS!Wörd9"

   '
   ' Known encrypted text to decrypt
   '
   Private Const ENCRYPTED_TEXT_V3 As String = "3$J/LJT9XGjwfmsKsvHzFefQ==$iJIhCFfmzwPVqDwJai30ei5WTpU3/7qhiBS7WbPQCCHJKppD06B2LsRP7tgqh+1g$C9mHKfJi5mdMdIOZWep2GhZl7fNk98c3fBD6j404RXY="
   Private Const SUBJECT As String = "maven_repo_pass"
   Private Const WRONG_SUBJECT As String = "maven_repo_paxx"
   Private Const ENCRYPTED_TEXT_V5 As String = "5$Qs6C7prscyK5/OiJRsjWtw$bobPzPN6BJI0Od9pMSUWrSXp5hm/U+0ihzrWH30wMhrZGFPGsnNl/Mv3xJLdHdE03PpD1CW99AK2IZKk006hVA$nP3mG9F4eKvYJoFEiOhMguzMbgpo7XR+JkNJnA6qdhQ"

   ''' <summary>
   ''' Known encrypted text to decrypt with invalid HMAC.
   ''' </summary>
   Private Const ENCRYPTED_TEXT_WITH_INVALID_HMAC As String = "3$J/LJT9XGjwfmsKsvHzFefQ==$iJIhCFfmzwPVqDwJai30ei5WTpU3/7qhiBS7WbPQCCHJKppD06B2LsRP7tgqh+1g$C9mHKfJi5mdMdIOZWep2GhZl7fNk98c3fBD6j404RXQ="

   ''' <summary>
   ''' Known encrypted text to decrypt with empty HMAC.
   ''' </summary>
   Private Const ENCRYPTED_TEXT_WITH_EMPTY_HMAC As String = "3$J/LJT9XGjwfmsKsvHzFefQ==$iJIhCFfmzwPVqDwJai30ei5WTpU3/7qhiBS7WbPQCCHJKppD06B2LsRP7tgqh+1g$"

   ''' <summary>
   ''' Known encrypted text to decrypt with invalid encryption.
   ''' </summary>
   Private Const ENCRYPTED_TEXT_WITH_INVALID_ENCRYPTION As String = "3$J/LJT9XGjwfmsKsvHzFefQ==$iJIhCFfmzwPVqDwJai30ei5WTpU3/7qhiBS7WbPQCCHJKppD06B2LsRP7tgqh+1Q$C9mHKfJi5mdMdIOZWep2GhZl7fNk98c3fBD6j404RXY="

   ''' <summary>
   ''' Known encrypted text to decrypt with invalid IV.
   ''' </summary>
   Private Const ENCRYPTED_TEXT_WITH_INVALID_IV As String = "3$J/LJT9XGjwfmsKsvHzFefz==$iJIhCFfmzwPVqDwJai30ei5WTpU3/7qhiBS7WbPQCCHJKppD06B2LsRP7tgqh+1g$C9mHKfJi5mdMdIOZWep2GhZl7fNk98c3fBD6j404RXY="

   ''' <summary>
   ''' Known encrypted text to decrypt with unknown format id.
   ''' </summary>
   Private Const ENCRYPTED_TEXT_WITH_UNKNOWN_FORMAT_ID As String = "99$J/LJT9XGjwfmsKsvHzFefQ==$iJIhCFfmzwPVqDwJai30ei5WTpU3/7qhiBS7WbPQCCHJKppD06B2LsRP7tgqh+1g$C9mHKfJi5mdMdIOZWep2GhZl7fNk98c3fBD6j404RXY="

   ''' <summary>
   ''' Known encrypted text to decrypt with invalid format id.
   ''' </summary>
   Private Const ENCRYPTED_TEXT_WITH_INVALID_FORMAT_ID As String = "Q$J/LJT9XGjwfmsKsvHzFefQ==$iJIhCFfmzwPVqDwJai30ei5WTpU3/7qhiBS7WbPQCCHJKppD06B2LsRP7tgqh+1g$C9mHKfJi5mdMdIOZWep2GhZl7fNk98c3fBD6j404RXY="

   ''' <summary>
   ''' Known encrypted text to decrypt with missing format id.
   ''' </summary>
   Private Const ENCRYPTED_TEXT_WITH_MISSING_FORMAT_ID As String = "J/LJT9XGjwfmsKsvHzFefQ==$iJIhCFfmzwPVqDwJai30ei5WTpU3/7qhiBS7WbPQCCHJKppD06B2LsRP7tgqh+1g$C9mHKfJi5mdMdIOZWep2GhZl7fNk98c3fBD6j404RXY="

   ''' <summary>
   ''' Known encrypted text to decrypt with empty IV.
   ''' </summary>
   Private Const ENCRYPTED_TEXT_WITH_EMPTY_IV As String = "3$$iJIhCFfmzwPVqDwJai30ei5WTpU3/7qhiBS7WbPQCCHJKppD06B2LsRP7tgqh+1g$C9mHKfJi5mdMdIOZWep2GhZl7fNk98c3fBD6j404RXY="

   ''' <summary>
   ''' Known encrypted text to decrypt with missing IV.
   ''' </summary>
   Private Const ENCRYPTED_TEXT_WITH_MISSING_IV As String = "3$iJIhCFfmzwPVqDwJai30ei5WTpU3/7qhiBS7WbPQCCHJKppD06B2LsRP7tgqh+1g$C9mHKfJi5mdMdIOZWep2GhZl7fNk98c3fBD6j404RXY="

#End Region

#Region "Constructor"
   Public Sub New()
      '
      ' Create a deterministic HMAC key from a pseudo-random number generator with a fixed key
      '
      Dim xs128 As XorRoShiRo128PlusPlus = New XorRoShiRo128PlusPlus(&HEBE770CC82F12283L)

      For i As Integer = 0 To COMPUTED_HMAC_KEY.Length - 1
         COMPUTED_HMAC_KEY(i) = xs128.GetByte()
      Next

      For i As Integer = 0 To SOURCE_BYTES_4.Length - 1
         SOURCE_BYTES_4(i) = xs128.GetByte()
      Next

      SOURCE_BYTES_1 = CHARACTER_ENCODING_FOR_DATA.GetBytes(SOURCE_TEXT_1)
      SOURCE_BYTES_2 = CHARACTER_ENCODING_FOR_DATA.GetBytes(SOURCE_TEXT_2)
      SOURCE_BYTES_3 = CHARACTER_ENCODING_FOR_DATA.GetBytes(SOURCE_TEXT_3)

      For i As Integer = 0 To NON_RANDOM_SOURCE_BYTES.Length - 1
         NON_RANDOM_SOURCE_BYTES(i) = CByte(&HFFI - (i And &HFFI))
      Next
   End Sub
#End Region

#Region "Test methods"
   ''' <summary>
   ''' Test if the encryption of a given byte array is correctly decrypted.
   ''' </summary>
   <TestMethod()> Public Sub TestEncryptionDecryptionForByteArray()
      Dim rnd As New Random()

      Dim myEncryptor As SplitKeyEncryption = Nothing

      Try
         myEncryptor = New SplitKeyEncryption(COMPUTED_HMAC_KEY, SOURCE_BYTES_1, SOURCE_BYTES_2, SOURCE_BYTES_3, SOURCE_BYTES_4)

         For i As Byte = 1 To 100
            Dim testByteArray As Byte() = New Byte(0 To rnd.Next(500)) {}

            rnd.NextBytes(testByteArray)

            Dim encryptedText As String = myEncryptor.EncryptData(testByteArray)

            Dim decryptedByteArray As Byte() = myEncryptor.DecryptDataAsByteArray(encryptedText)

            Assert.IsTrue(ArrayHelper.AreEqual(testByteArray, decryptedByteArray), "Decrypted byte array is not the same as original byte array")
         Next i

         myEncryptor.Dispose()

      Catch ex As Exception
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

         Assert.Fail(GetUnexpectedExceptionMessage(ex))
      End Try
   End Sub

   ''' <summary>
   ''' Test if the encryption of a given character array Is correctly decrypted.
   ''' </summary>
   <TestMethod()> Public Sub TestEncryptionDecryptionForCharacterArray()
      Dim myEncryptor As SplitKeyEncryption = Nothing

      Try
         Dim testCharArray As Char() = {"T"c, "h"c, "í"c, "s"c, " "c, "ì"c, "s"c, " "c, "a"c, " "c, "T"c, "ä"c, "s"c, "t"c}

         myEncryptor = New SplitKeyEncryption(COMPUTED_HMAC_KEY, SOURCE_BYTES_1, SOURCE_BYTES_2, SOURCE_BYTES_3, SOURCE_BYTES_4)

         Dim encryptedText As String = myEncryptor.EncryptData(testCharArray)

         Dim decryptedCharArray As Char() = myEncryptor.DecryptDataAsCharacterArray(encryptedText)

         myEncryptor.Dispose()

         Assert.IsTrue(ArrayHelper.AreEqual(testCharArray, decryptedCharArray), "Decrypted chararacter array is not the same as original character array")

      Catch ex As Exception
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

         Assert.Fail(GetUnexpectedExceptionMessage(ex))
      End Try
   End Sub

   ''' <summary>
   ''' Test if the encryption of a given string is correctly decrypted.
   ''' </summary>
   <TestMethod()> Public Sub TestEncryptionDecryptionForString()
      Dim myEncryptor As SplitKeyEncryption = Nothing

      Try
         myEncryptor = New SplitKeyEncryption(COMPUTED_HMAC_KEY, SOURCE_BYTES_1, SOURCE_BYTES_2, SOURCE_BYTES_3, SOURCE_BYTES_4)

         Dim encryptedText As String = myEncryptor.EncryptData(CLEAR_TEXT_V5)

         Dim decryptedString As String = myEncryptor.DecryptDataAsString(encryptedText)

         myEncryptor.Dispose()

         Assert.AreEqual(CLEAR_TEXT_V5, decryptedString, TEXT_DECRYPTION_MISMATCH_MESSAGE)

      Catch ex As Exception
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

         Assert.Fail(GetUnexpectedExceptionMessage(ex))
      End Try
   End Sub

   ''' <summary>
   ''' Test if the encryption of a given text is correctly decrypted with a subject present.
   ''' </summary>
   <TestMethod()> Public Sub TestEncryptionDecryptionWithSubject()
      Dim myEncryptor As SplitKeyEncryption = Nothing

      Try
         myEncryptor = New SplitKeyEncryption(COMPUTED_HMAC_KEY, SOURCE_BYTES_1, SOURCE_BYTES_2, SOURCE_BYTES_3, SOURCE_BYTES_4)

         Dim encryptedText As String = myEncryptor.EncryptData(CLEAR_TEXT_V5, SUBJECT)

         Dim decryptedString As String = myEncryptor.DecryptDataAsString(encryptedText, SUBJECT)

         myEncryptor.Dispose()

         Assert.AreEqual(CLEAR_TEXT_V5, decryptedString, TEXT_DECRYPTION_MISMATCH_MESSAGE)

      Catch ex As Exception
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

         Assert.Fail(GetUnexpectedExceptionMessage(ex))
      End Try
   End Sub

   ''' <summary>
   ''' Test if the encryption of an empty string is correctly decrypted.
   ''' </summary>
   <TestMethod()> Public Sub TestEmptyEncryptionDecryption()
      Dim myEncryptor As SplitKeyEncryption = Nothing

      Try
         Dim emptyString As String = ""

         myEncryptor = New SplitKeyEncryption(COMPUTED_HMAC_KEY, SOURCE_BYTES_1, SOURCE_BYTES_2, SOURCE_BYTES_3, SOURCE_BYTES_4)

         Dim encryptedText As String = myEncryptor.EncryptData(emptyString)

         Dim decryptedString As String = myEncryptor.DecryptDataAsString(encryptedText)

         myEncryptor.Dispose()

         Assert.AreEqual(emptyString, decryptedString, TEXT_DECRYPTION_MISMATCH_MESSAGE)

      Catch ex As Exception
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

         Assert.Fail(GetUnexpectedExceptionMessage(ex))
      End Try
   End Sub

   ''' <summary>
   ''' Test if the encryption of an empty string is correctly decrypted with a subject present.
   ''' </summary>
   <TestMethod()> Public Sub TestEmptyEncryptionDecryptionWithSubject()
      Dim myEncryptor As SplitKeyEncryption = Nothing

      Try
         Dim emptyString As String = ""

         myEncryptor = New SplitKeyEncryption(COMPUTED_HMAC_KEY, SOURCE_BYTES_1, SOURCE_BYTES_2, SOURCE_BYTES_3, SOURCE_BYTES_4)

         Dim encryptedText As String = myEncryptor.EncryptData(emptyString, SUBJECT)

         Dim decryptedString As String = myEncryptor.DecryptDataAsString(encryptedText, SUBJECT)

         myEncryptor.Dispose()

         Assert.AreEqual(emptyString, decryptedString, TEXT_DECRYPTION_MISMATCH_MESSAGE)

      Catch ex As Exception
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

         Assert.Fail(GetUnexpectedExceptionMessage(ex))
      End Try
   End Sub

   ''' <summary>
   ''' Test if a known encrypted text is correctly decrypted.
   ''' </summary>
   <TestMethod()> Public Sub TestKnownDecryption()
      Dim myEncryptor As SplitKeyEncryption = Nothing

      Try
         myEncryptor = New SplitKeyEncryption(CONSTANT_HMAC_KEY, NON_RANDOM_SOURCE_BYTES)

         Dim decryptedString As String = myEncryptor.DecryptDataAsString(ENCRYPTED_TEXT_V3)

         myEncryptor.Dispose()

         Assert.AreEqual(CLEAR_TEXT_V3, decryptedString, TEXT_DECRYPTION_MISMATCH_MESSAGE)

      Catch ex As Exception
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

         Assert.Fail(GetUnexpectedExceptionMessage(ex))
      End Try
   End Sub

   ''' <summary>
   ''' Test if a given encrypted text is correctly decrypted.
   ''' </summary>
   <TestMethod()> Public Sub TestKnownDecryptionWithSubject()
      Dim myEncryptor As SplitKeyEncryption = Nothing

      Try
         myEncryptor = New SplitKeyEncryption(CONSTANT_HMAC_KEY, NON_RANDOM_SOURCE_BYTES)

         Dim decryptedString As String = myEncryptor.DecryptDataAsString(ENCRYPTED_TEXT_V5, SUBJECT)

         myEncryptor.Dispose()

         Assert.AreEqual(CLEAR_TEXT_V5, decryptedString, TEXT_DECRYPTION_MISMATCH_MESSAGE)

      Catch ex As Exception
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

         Assert.Fail(GetUnexpectedExceptionMessage(ex))
      End Try
   End Sub

   ''' <summary>
   ''' Test if the decryption of a byte array throws an exception if decrypted as a character array.
   ''' </summary>
   <TestMethod()> Public Sub TestDecryptionToCharArrayWithInvalidByteArray()
      Dim myEncryptor As SplitKeyEncryption = Nothing

      Try
         Dim testByteArray As Byte() = New Byte(0 To 255) {}

         For i As Integer = 0 To testByteArray.Length - 1
            testByteArray(i) = CByte(&HFF - i)
         Next

         myEncryptor = New SplitKeyEncryption(COMPUTED_HMAC_KEY, SOURCE_BYTES_1, SOURCE_BYTES_2, SOURCE_BYTES_3, SOURCE_BYTES_4)

         Dim encryptedText As String = myEncryptor.EncryptData(testByteArray, SUBJECT)

         ' This must throw an exception as the original byte array is not a valid UTF-8 encoding
#Disable Warning S1481 ' Unused local variables should be removed
         Dim decryptedCharacterArray As Char() = myEncryptor.DecryptDataAsCharacterArray(encryptedText, SUBJECT)
#Enable Warning S1481 ' Unused local variables should be removed

         myEncryptor.Dispose()

         Assert.Fail(TEXT_EXPECTED_EXCEPTION)

      Catch ex As ArgumentException
         '
         ' This is the expected exception
         '
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

         Assert.IsTrue(ex.Message().Contains("Unicode"), GetUnexpectedExceptionMessage(ex))

      Catch ex As Exception
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

         Assert.Fail(GetUnexpectedExceptionMessage(ex))
      End Try
   End Sub

   ''' <summary>
   ''' Test if the decryption of a byte array throws an exception if decrypted as a string.
   ''' </summary>
   <TestMethod()> Public Sub TestDecryptionToStringWithInvalidByteArray()
      Dim myEncryptor As SplitKeyEncryption = Nothing

      Try
         Dim testByteArray As Byte() = New Byte(0 To 255) {}

         For i As Integer = 0 To testByteArray.Length - 1
            testByteArray(i) = CByte(&HFF - i)
         Next

         myEncryptor = New SplitKeyEncryption(COMPUTED_HMAC_KEY, SOURCE_BYTES_1, SOURCE_BYTES_2, SOURCE_BYTES_3, SOURCE_BYTES_4)

         Dim encryptedText As String = myEncryptor.EncryptData(testByteArray, SUBJECT)

         ' This must throw an exception as the original byte array is not a valid UTF-8 encoding
#Disable Warning S1481 ' Unused local variables should be removed
         Dim decryptedString As String = myEncryptor.DecryptDataAsString(encryptedText, SUBJECT)
#Enable Warning S1481 ' Unused local variables should be removed

         myEncryptor.Dispose()

         Assert.Fail(TEXT_EXPECTED_EXCEPTION)

      Catch ex As ArgumentException
         '
         ' This is the expected exception
         '
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

         Assert.IsTrue(ex.Message().Contains("Unicode"), GetUnexpectedExceptionMessage(ex))

      Catch ex As Exception
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

         Assert.Fail(GetUnexpectedExceptionMessage(ex))
      End Try
   End Sub

   ''' <summary>
   ''' Test if a given encrypted text with the wrong subject throws an exception.
   ''' </summary>
   <TestMethod()> Public Sub TestDecryptionWithWrongSubject()
      Dim myEncryptor As SplitKeyEncryption = Nothing

      Try
         myEncryptor = New SplitKeyEncryption(CONSTANT_HMAC_KEY, NON_RANDOM_SOURCE_BYTES)

         Dim encryptedText As String = myEncryptor.EncryptData(ENCRYPTED_TEXT_V5)

#Disable Warning S1481 ' Unused local variables should be removed
         Dim decryptedString As String = myEncryptor.DecryptDataAsString(encryptedText, WRONG_SUBJECT)
#Enable Warning S1481 ' Unused local variables should be removed

         myEncryptor.Dispose()

         Assert.Fail(TEXT_EXPECTED_EXCEPTION)

      Catch ex As DataIntegrityException
         '
         ' This is the expected exception
         '
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

      Catch ex As Exception
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

         Assert.Fail(GetUnexpectedExceptionMessage(ex))
      End Try
   End Sub

   ''' <summary>
   ''' Test if a given encrypted text with an invalid HMAC throws an exception.
   ''' </summary>
   <TestMethod()> Public Sub TestKnownDecryptionWithInvalidHMAC()
      Dim myEncryptor As SplitKeyEncryption = Nothing

      Try
         myEncryptor = New SplitKeyEncryption(CONSTANT_HMAC_KEY, NON_RANDOM_SOURCE_BYTES)

#Disable Warning S1481 ' Unused local variables should be removed
         Dim decryptedString As String = myEncryptor.DecryptDataAsString(ENCRYPTED_TEXT_WITH_INVALID_HMAC)
#Enable Warning S1481 ' Unused local variables should be removed

         myEncryptor.Dispose()

         Assert.Fail(TEXT_EXPECTED_EXCEPTION)

      Catch ex As DataIntegrityException
         '
         ' This is the expected exception
         '
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

      Catch ex As Exception
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

         Assert.Fail(GetUnexpectedExceptionMessage(ex))
      End Try
   End Sub

   ''' <summary>
   ''' Test if a given encrypted text with an empty HMAC throws an exception.
   ''' </summary>
   <TestMethod()> Public Sub TestKnownDecryptionWithEmptyHMAC()
      Dim myEncryptor As SplitKeyEncryption = Nothing

      Try
         myEncryptor = New SplitKeyEncryption(CONSTANT_HMAC_KEY, NON_RANDOM_SOURCE_BYTES)

#Disable Warning S1481 ' Unused local variables should be removed
         Dim decryptedString As String = myEncryptor.DecryptDataAsString(ENCRYPTED_TEXT_WITH_EMPTY_HMAC)
#Enable Warning S1481 ' Unused local variables should be removed

         myEncryptor.Dispose()

         Assert.Fail(TEXT_EXPECTED_EXCEPTION)

      Catch ex As DataIntegrityException
         '
         ' This is the expected exception
         '
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

      Catch ex As Exception
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

         Assert.Fail(GetUnexpectedExceptionMessage(ex))
      End Try
   End Sub

   ''' <summary>
   ''' Test if a given encrypted text with an invalid encryption part throws an exception.
   ''' </summary>
   <TestMethod()> Public Sub TestKnownDecryptionWithInvalidEncryption()
      Dim myEncryptor As SplitKeyEncryption = Nothing

      Try
         myEncryptor = New SplitKeyEncryption(CONSTANT_HMAC_KEY, NON_RANDOM_SOURCE_BYTES)

#Disable Warning S1481 ' Unused local variables should be removed
         Dim decryptedString As String = myEncryptor.DecryptDataAsString(ENCRYPTED_TEXT_WITH_INVALID_ENCRYPTION)
#Enable Warning S1481 ' Unused local variables should be removed

         myEncryptor.Dispose()

         Assert.Fail(TEXT_EXPECTED_EXCEPTION)

      Catch ex As DataIntegrityException
         '
         ' This is the expected exception
         '
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

      Catch ex As Exception
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

         Assert.Fail(GetUnexpectedExceptionMessage(ex))
      End Try
   End Sub

   ''' <summary>
   ''' Test if a given encrypted text with an invalid IV throws an exception.
   ''' </summary>
   <TestMethod()> Public Sub TestKnownDecryptionWithInvalidIV()
      Dim myEncryptor As SplitKeyEncryption = Nothing

      Try
         myEncryptor = New SplitKeyEncryption(CONSTANT_HMAC_KEY, NON_RANDOM_SOURCE_BYTES)

#Disable Warning S1481 ' Unused local variables should be removed
         Dim decryptedString As String = myEncryptor.DecryptDataAsString(ENCRYPTED_TEXT_WITH_INVALID_IV)
#Enable Warning S1481 ' Unused local variables should be removed

         myEncryptor.Dispose()

         Assert.Fail(TEXT_EXPECTED_EXCEPTION)

      Catch ex As DataIntegrityException
         '
         ' This is the expected exception
         '
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

      Catch ex As Exception
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

         Assert.Fail(GetUnexpectedExceptionMessage(ex))
      End Try
   End Sub

   ''' <summary>
   ''' Test if a given encrypted text with an unknown format id throws an exception.
   ''' </summary>
   <TestMethod()> Public Sub TestKnownDecryptionWithUnknownFormatId()
      Dim myEncryptor As SplitKeyEncryption = Nothing

      Try
         myEncryptor = New SplitKeyEncryption(CONSTANT_HMAC_KEY, NON_RANDOM_SOURCE_BYTES)

#Disable Warning S1481 ' Unused local variables should be removed
         Dim decryptedString As String = myEncryptor.DecryptDataAsString(ENCRYPTED_TEXT_WITH_UNKNOWN_FORMAT_ID)
#Enable Warning S1481 ' Unused local variables should be removed

         myEncryptor.Dispose()

         Assert.Fail(TEXT_EXPECTED_EXCEPTION)

      Catch ex As ArgumentException
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

         Assert.IsTrue(ex.Message().Contains("Unknown format id"), GetUnexpectedExceptionMessage(ex))

      Catch ex As Exception
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

         Assert.Fail(GetUnexpectedExceptionMessage(ex))
      End Try
   End Sub

   ''' <summary>
   ''' Test if a given encrypted text with an invalid format id throws an exception.
   ''' </summary>
   <TestMethod()> Public Sub TestKnownDecryptionWithInvalidFormatId()
      Dim myEncryptor As SplitKeyEncryption = Nothing

      Try
         myEncryptor = New SplitKeyEncryption(CONSTANT_HMAC_KEY, NON_RANDOM_SOURCE_BYTES)

#Disable Warning S1481 ' Unused local variables should be removed
         Dim decryptedString As String = myEncryptor.DecryptDataAsString(ENCRYPTED_TEXT_WITH_INVALID_FORMAT_ID)
#Enable Warning S1481 ' Unused local variables should be removed

         myEncryptor.Dispose()

         Assert.Fail(TEXT_EXPECTED_EXCEPTION)

      Catch ex As ArgumentException
         '
         ' This is the expected exception
         '
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

         Assert.IsTrue(ex.Message().Contains("Invalid format id"), GetUnexpectedExceptionMessage(ex))

      Catch ex As Exception
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

         Assert.Fail(GetUnexpectedExceptionMessage(ex))
      End Try
   End Sub

   ''' <summary>
   ''' Test if a given encrypted text with a missing format id throws an exception.
   ''' </summary>
   <TestMethod()> Public Sub TestKnownDecryptionWithMissingFormatId()
      Dim myEncryptor As SplitKeyEncryption = Nothing

      Try
         myEncryptor = New SplitKeyEncryption(CONSTANT_HMAC_KEY, NON_RANDOM_SOURCE_BYTES)

#Disable Warning S1481 ' Unused local variables should be removed
         Dim decryptedString As String = myEncryptor.DecryptDataAsString(ENCRYPTED_TEXT_WITH_MISSING_FORMAT_ID)
#Enable Warning S1481 ' Unused local variables should be removed

         myEncryptor.Dispose()

         Assert.Fail(TEXT_EXPECTED_EXCEPTION)

      Catch ex As ArgumentException
         '
         ' This is the expected exception
         '
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

         Assert.IsTrue(ex.Message().Contains("Invalid format id"), GetUnexpectedExceptionMessage(ex))

      Catch ex As Exception
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

         Assert.Fail(GetUnexpectedExceptionMessage(ex))
      End Try
   End Sub

   ''' <summary>
   ''' Test if a given encrypted text with an empty IV throws an exception.
   ''' </summary>
   <TestMethod()> Public Sub TestKnownDecryptionWithEmptyIV()
      Dim myEncryptor As SplitKeyEncryption = Nothing

      Try
         myEncryptor = New SplitKeyEncryption(CONSTANT_HMAC_KEY, NON_RANDOM_SOURCE_BYTES)

#Disable Warning S1481 ' Unused local variables should be removed
         Dim decryptedString As String = myEncryptor.DecryptDataAsString(ENCRYPTED_TEXT_WITH_EMPTY_IV)
#Enable Warning S1481 ' Unused local variables should be removed

         myEncryptor.Dispose()

         Assert.Fail(TEXT_EXPECTED_EXCEPTION)

      Catch ex As DataIntegrityException
         '
         ' This is the expected exception
         '
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

      Catch ex As Exception
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

         Assert.Fail(GetUnexpectedExceptionMessage(ex))
      End Try
   End Sub

   ''' <summary>
   ''' Test if a given encrypted text with a missing IV throws an exception
   ''' </summary>
   <TestMethod()> Public Sub TestKnownDecryptionWithMissingIV()
      Dim myEncryptor As SplitKeyEncryption = Nothing

      Try
         myEncryptor = New SplitKeyEncryption(CONSTANT_HMAC_KEY, NON_RANDOM_SOURCE_BYTES)

#Disable Warning S1481 ' Unused local variables should be removed
         Dim decryptedString As String = myEncryptor.DecryptDataAsString(ENCRYPTED_TEXT_WITH_MISSING_IV)
#Enable Warning S1481 ' Unused local variables should be removed

         myEncryptor.Dispose()

         Assert.Fail(TEXT_EXPECTED_EXCEPTION)

      Catch ex As ArgumentException
         '
         ' This is the expected exception
         '
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

         Assert.IsTrue(ex.Message().Contains("separated parts in encrypted text is not"), GetUnexpectedExceptionMessage(ex))

      Catch ex As Exception
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

         Assert.Fail(GetUnexpectedExceptionMessage(ex))
      End Try
   End Sub

   ''' <summary>
   ''' Test if an empty HMAC in the constructor throws an exception.
   ''' </summary>
   <TestMethod()> Public Sub TestKnownDecryptionWithEmptyHMACInConstructor()
      Dim myEncryptor As SplitKeyEncryption = Nothing

      Try
         Dim emptyHmac As Byte() = Array.Empty(Of Byte)

         myEncryptor = New SplitKeyEncryption(emptyHmac, SOURCE_BYTES_1, SOURCE_BYTES_2, SOURCE_BYTES_3, SOURCE_BYTES_4)

#Disable Warning S1481 ' Unused local variables should be removed
         Dim encryptedText As String = myEncryptor.EncryptData(CLEAR_TEXT_V5)
#Enable Warning S1481 ' Unused local variables should be removed

         myEncryptor.Dispose()

         Assert.Fail(TEXT_EXPECTED_EXCEPTION)

      Catch ex As ArgumentException
         '
         ' This is the expected exception
         '
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

         Assert.IsTrue(ex.Message().Contains("HMAC key length is less than"), GetUnexpectedExceptionMessage(ex))

      Catch ex As Exception
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

         Assert.Fail(GetUnexpectedExceptionMessage(ex))
      End Try
   End Sub

   ''' <summary>
   ''' Test if a too short HMAC in the constructor throws an exception.
   ''' </summary>
   <TestMethod()> Public Sub TestKnownDecryptionWithShortHMACInConstructor()
      Dim myEncryptor As SplitKeyEncryption = Nothing

      Try
         Dim shortHmac As Byte() = New Byte(0 To 9) {}

         myEncryptor = New SplitKeyEncryption(shortHmac, SOURCE_BYTES_1, SOURCE_BYTES_2, SOURCE_BYTES_3, SOURCE_BYTES_4)

#Disable Warning S1481 ' Unused local variables should be removed
         Dim encryptedText As String = myEncryptor.EncryptData(CLEAR_TEXT_V5)
#Enable Warning S1481 ' Unused local variables should be removed

         myEncryptor.Dispose()

         Assert.Fail(TEXT_EXPECTED_EXCEPTION)

      Catch ex As ArgumentException
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

         Assert.IsTrue(ex.Message().Contains("HMAC key length is less than"), GetUnexpectedExceptionMessage(ex))

      Catch ex As Exception
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

         Assert.Fail(GetUnexpectedExceptionMessage(ex))
      End Try
   End Sub

   ''' <summary>
   ''' Test if a too large HMAC in the constructor throws an exception.
   ''' </summary>
   <TestMethod()> Public Sub TestKnownDecryptionWithTooLargeHMACInConstructor()
      Dim myEncryptor As SplitKeyEncryption = Nothing

      Try
         Dim largeHmac As Byte() = New Byte(0 To 69) {}

         myEncryptor = New SplitKeyEncryption(largeHmac, SOURCE_BYTES_1, SOURCE_BYTES_2, SOURCE_BYTES_3, SOURCE_BYTES_4)

#Disable Warning S1481 ' Unused local variables should be removed
         Dim encryptedText As String = myEncryptor.EncryptData(CLEAR_TEXT_V5)
#Enable Warning S1481 ' Unused local variables should be removed

         myEncryptor.Dispose()

         Assert.Fail(TEXT_EXPECTED_EXCEPTION)

      Catch ex As ArgumentException
         '
         ' This is the expected exception
         '
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

         Assert.IsTrue(ex.Message().Contains("HMAC key length is larger than"), GetUnexpectedExceptionMessage(ex))

      Catch ex As Exception
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

         Assert.Fail(GetUnexpectedExceptionMessage(ex))
      End Try
   End Sub

   ''' <summary>
   ''' Test if null HMAC key in the constructor throws an exception.
   ''' </summary>
   <TestMethod()> Public Sub TestNullHMACKeyInConstructor()
      Dim myEncryptor As SplitKeyEncryption = Nothing

      Try
         Dim nullHmac As Byte() = Nothing

         myEncryptor = New SplitKeyEncryption(nullHmac, SOURCE_BYTES_1, SOURCE_BYTES_2, SOURCE_BYTES_3, SOURCE_BYTES_4)

#Disable Warning S1481 ' Unused local variables should be removed
         Dim encryptedText As String = myEncryptor.EncryptData(CLEAR_TEXT_V5)
#Enable Warning S1481 ' Unused local variables should be removed

         myEncryptor.Dispose()

         Assert.Fail(TEXT_EXPECTED_EXCEPTION)

      Catch ex As ArgumentNullException
         '
         ' This is the expected exception
         '
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

         Assert.IsTrue(ex.Message().Contains("aHMACKey"), GetUnexpectedExceptionMessage(ex))

      Catch ex As Exception
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

         Assert.Fail(GetUnexpectedExceptionMessage(ex))
      End Try
   End Sub

   ''' <summary>
   ''' Test if a null byte array in the source bytes throws an exception.
   ''' </summary>
   <TestMethod()> Public Sub TestNullByteArrayInSource()
      Dim myEncryptor As SplitKeyEncryption = Nothing

      Try
         Dim nullSourceByteArray As Byte() = Nothing

         myEncryptor = New SplitKeyEncryption(COMPUTED_HMAC_KEY, SOURCE_BYTES_1, nullSourceByteArray, SOURCE_BYTES_3, SOURCE_BYTES_4)

#Disable Warning S1481 ' Unused local variables should be removed
         Dim encryptedText As String = myEncryptor.EncryptData(CLEAR_TEXT_V5)
#Enable Warning S1481 ' Unused local variables should be removed

         myEncryptor.Dispose()

         Assert.Fail(TEXT_EXPECTED_EXCEPTION)

      Catch ex As ArgumentNullException
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

         Assert.IsTrue(ex.Message().Contains("sourceBytes(1)"), GetUnexpectedExceptionMessage(ex))

      Catch ex As Exception
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

         Assert.Fail(GetUnexpectedExceptionMessage(ex))
      End Try
   End Sub

   ''' <summary>
   ''' Test if a very short source byte array throws an exception.
   ''' </summary>
   <TestMethod()> Public Sub TestShortSourceBytes()
      Dim myEncryptor As SplitKeyEncryption = Nothing

      Try
         Dim shortSourceByteArray As Byte() = New Byte() {&HAA, &HBB, &HCC}

         myEncryptor = New SplitKeyEncryption(COMPUTED_HMAC_KEY, shortSourceByteArray)

#Disable Warning S1481 ' Unused local variables should be removed
         Dim encryptedText As String = myEncryptor.EncryptData(CLEAR_TEXT_V5)
#Enable Warning S1481 ' Unused local variables should be removed

         myEncryptor.Dispose()

         Assert.Fail(TEXT_EXPECTED_EXCEPTION)

      Catch ex As ArgumentException
         '
         ' This is the expected exception
         '
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

         Assert.IsTrue(ex.Message().Contains("There is not enough information provided in the source bytes"), GetUnexpectedExceptionMessage(ex))

      Catch ex As Exception
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

         Assert.Fail(GetUnexpectedExceptionMessage(ex))
      End Try
   End Sub

   ''' <summary>
   ''' Test if a uniform source byte array throws an exception.
   ''' </summary>
   <TestMethod()> Public Sub TestUniformSourceBytes()
      Dim myEncryptor As SplitKeyEncryption = Nothing

      Try
         Dim uniformSourceByteArray As Byte() = New Byte(0 To 299) {}

         ArrayHelper.Fill(uniformSourceByteArray, &HAA)

         myEncryptor = New SplitKeyEncryption(COMPUTED_HMAC_KEY, uniformSourceByteArray)

#Disable Warning S1481 ' Unused local variables should be removed
         Dim encryptedText As String = myEncryptor.EncryptData(CLEAR_TEXT_V5)
#Enable Warning S1481 ' Unused local variables should be removed

         myEncryptor.Dispose()

         Assert.Fail(TEXT_EXPECTED_EXCEPTION)

      Catch ex As ArgumentException
         '
         ' This is the expected exception
         '
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

         Assert.IsTrue(ex.Message().Contains("There is no information provided in the source bytes"), GetUnexpectedExceptionMessage(ex))

      Catch ex As Exception
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

         Assert.Fail(GetUnexpectedExceptionMessage(ex))
      End Try
   End Sub

   ''' <summary>
   ''' Test if a nearly uniform source byte array throws an exception.
   ''' </summary> 
   <TestMethod()> Public Sub TestNearlyUniformSourceBytes()
      Dim nearlyUniformSourceByteArray As Byte() = New Byte(0 To 99) {}

      For i = 0 To nearlyUniformSourceByteArray.Length - 1
         If (i And 1) <> 0 Then
            nearlyUniformSourceByteArray(i) = &H55
         Else
            nearlyUniformSourceByteArray(i) = &HAA
         End If
      Next

      Dim myEncryptor As SplitKeyEncryption = Nothing

      Try
         myEncryptor = New SplitKeyEncryption(COMPUTED_HMAC_KEY, nearlyUniformSourceByteArray)

#Disable Warning S1481 ' Unused local variables should be removed
         Dim encryptedText As String = myEncryptor.EncryptData(CLEAR_TEXT_V5)
#Enable Warning S1481 ' Unused local variables should be removed

         myEncryptor.Dispose()

         Assert.Fail(TEXT_EXPECTED_EXCEPTION)

      Catch ex As ArgumentException
         '
         ' This is the expected exception
         '
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

         Assert.IsTrue(ex.Message().Contains("There is not enough information provided in the source bytes"), GetUnexpectedExceptionMessage(ex))

      Catch ex As Exception
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

         Assert.Fail(GetUnexpectedExceptionMessage(ex))
      End Try
   End Sub
#End Region

#Region "Private methods"
   ''' <summary>
   ''' Get text for unexpected exceptions.
   ''' </summary>
   ''' <param name="ex">The unexpected exception</param>
   ''' <returns>Text that describes the unexpected exception.</returns>
   Private Function GetUnexpectedExceptionMessage(ex As Exception) As String
      Return TEXT_UNEXPECTED_EXCEPTION & ex.ToString() & TEXT_SEPARATOR & ex.Message() & TEXT_SEPARATOR & ex.StackTrace()
   End Function
#End Region
End Class