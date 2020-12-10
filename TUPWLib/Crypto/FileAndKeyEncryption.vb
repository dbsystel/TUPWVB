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
' Version: 1.0.0
'
' Change history:
'    2020-05-05: V1.0.0: Created.
'    2020-12-10: V2.0.0: Correct handling of disposed instances.
'

Imports System.IO

''' <summary>
''' Provides encryption by key generated from a file and a key
''' </summary>
Public Class FileAndKeyEncryption : Implements IDisposable
#Region "Instance variables"
   '******************************************************************
   ' Instance variables
   '******************************************************************

   ''' <summary>
   ''' Instance of <see cref="SplitKeyEncryption"/> to use behind this interface.
   ''' </summary>
   Private ReadOnly m_SplitKeyEncryption As SplitKeyEncryption
#End Region

#Region "Constructors"
   '******************************************************************
   ' Constructor
   '******************************************************************

   ''' <summary>
   ''' Constructor for this instance.
   ''' </summary>
   ''' <param name="hmacKey">Key for the HMAC of the file.</param>
   ''' <param name="keyFilePath">Path of key file.</param>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="keyFilePath"/> contains invalid characters.</exception>
   ''' <exception cref="ArgumentNullException">Thrown if any parameter is <c>Nothing</c>.</exception>
   ''' <exception cref="DirectoryNotFoundException">Thrown if the directory in <paramref name="keyFilePath"/> does not exist.</exception>
   ''' <exception cref="FileNotFoundException">Thrown if <paramref name="keyFilePath"/> is not found.</exception>
   ''' <exception cref="IOException">Thrown if an I/O error occurs during access to the file.</exception>
   ''' <exception cref="NotSupportedException">Thrown if <paramref name="keyFilePath"/> is in an invalid format.</exception>
   ''' <exception cref="PathTooLongException">Thrown if <paramref name="keyFilePath"/> is too long.</exception>
   ''' <exception cref="UnauthorizedAccessException">Thrown if <paramref name="keyFilePath"/> specifies a directory or the caller
   ''' does not have permission to read the file.</exception>
   Public Sub New(hmacKey As Byte(), keyFilePath As String)
      RequireNonNull(hmacKey, NameOf(hmacKey))
      RequireNonNull(keyFilePath, NameOf(keyFilePath))

      Dim keyFileBytes As Byte() = GetContentOfFile(keyFilePath)

      m_SplitKeyEncryption = New SplitKeyEncryption(hmacKey, keyFileBytes)

      ArrayHelper.Clear(keyFileBytes)
   End Sub
#End Region

#Region "Public attributes"
   ''' <summary>
   ''' Checks whether this instance is valid
   ''' </summary>
   ''' <returns><c>true</c>, if this instance is in a valid state, <c>false</c>, if this instance has already been disposed of.</returns>
   Public ReadOnly Property IsValid As Boolean
      Get
         Return Not m_IsDisposed
      End Get
   End Property
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
   Public Function EncryptData(byteArrayToEncrypt As Byte(), subject As String) As String
      Return m_SplitKeyEncryption.EncryptData(byteArrayToEncrypt, subject)
   End Function

   ''' <summary>
   ''' Encrypt a byte array.
   ''' </summary>
   ''' <param name="byteArrayToEncrypt">Byte array to encrypt.</param>
   ''' <returns>Printable form of the encrypted string.</returns>
   Public Function EncryptData(byteArrayToEncrypt As Byte()) As String
      Return m_SplitKeyEncryption.EncryptData(byteArrayToEncrypt)
   End Function

   ''' <summary>
   ''' Encrypt a character array under a subject.
   ''' </summary>
   ''' <param name="characterArrayToEncrypt">Character array to encrypt.</param>
   ''' <param name="subject">The subject of this encryption.</param>
   ''' <returns>Printable form of the encrypted string.</returns>
   Public Function EncryptData(characterArrayToEncrypt As Char(), subject As String) As String
      Return m_SplitKeyEncryption.EncryptData(characterArrayToEncrypt, subject)
   End Function

   ''' <summary>
   ''' Encrypt a character array.
   ''' </summary>
   ''' <param name="characterArrayToEncrypt">Character array to encrypt.</param>
   ''' <returns>Printable form of the encrypted string.</returns>
   Public Function EncryptData(characterArrayToEncrypt As Char()) As String
      Return m_SplitKeyEncryption.EncryptData(characterArrayToEncrypt)
   End Function

   ''' <summary>
   ''' Encrypt a string under a subject.
   ''' </summary>
   ''' <param name="stringToEncrypt">String to encrypt.</param>
   ''' <param name="subject">The subject of this encryption.</param>
   ''' <returns>Printable form of the encrypted string.</returns>
   Public Function EncryptData(stringToEncrypt As String, subject As String) As String
      Return m_SplitKeyEncryption.EncryptData(stringToEncrypt, subject)
   End Function

   ''' <summary>
   ''' Encrypt a string.
   ''' </summary>
   ''' <param name="stringToEncrypt">String to encrypt.</param>
   ''' <returns>Printable form of the encrypted string.</returns>
   Public Function EncryptData(stringToEncrypt As String) As String
      Return m_SplitKeyEncryption.EncryptData(stringToEncrypt)
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
   Public Function DecryptDataAsByteArray(stringToDecrypt As String, subject As String) As Byte()
      Return m_SplitKeyEncryption.DecryptDataAsByteArray(stringToDecrypt, subject)
   End Function

   ''' <summary>
   ''' Decrypt an encrypted string and return a byte array.
   ''' </summary>
   ''' <param name="stringToDecrypt">String to decrypt.</param>
   ''' <returns>Decrypted string as a byte array.</returns>
   Public Function DecryptDataAsByteArray(stringToDecrypt As String) As Byte()
      Return m_SplitKeyEncryption.DecryptDataAsByteArray(stringToDecrypt)
   End Function

   ''' <summary>
   ''' Decrypt an encrypted string under a subject and return a character array.
   ''' </summary>
   ''' <param name="stringToDecrypt">String to decrypt.</param>
   ''' <param name="subject">The subject of this decryption.</param>
   ''' <returns>Decrypted string as a character array.</returns>
   Public Function DecryptDataAsCharacterArray(stringToDecrypt As String, subject As String) As Char()
      Return m_SplitKeyEncryption.DecryptDataAsCharacterArray(stringToDecrypt, subject)
   End Function

   ''' <summary>
   ''' Decrypt an encrypted string and return a character array.
   ''' </summary>
   ''' <param name="stringToDecrypt">String to decrypt.</param>
   ''' <returns>Decrypted string as a character array.</returns>
   Public Function DecryptDataAsCharacterArray(stringToDecrypt As String) As Char()
      Return m_SplitKeyEncryption.DecryptDataAsCharacterArray(stringToDecrypt)
   End Function

   ''' <summary>
   ''' Decrypt an encrypted string under a subject and return a string.
   ''' </summary>
   ''' <param name="stringToDecrypt">String to decrypt.</param>
   ''' <param name="subject">The subject of this decryption.</param>
   ''' <returns>Decrypted string as a string.</returns>
   Public Function DecryptDataAsString(stringToDecrypt As String, subject As String) As String
      Return m_SplitKeyEncryption.DecryptDataAsString(stringToDecrypt, subject)
   End Function

   ''' <summary>
   ''' Decrypt an encrypted string and return a string.
   ''' </summary>
   ''' <param name="stringToDecrypt">String to decrypt.</param>
   ''' <returns>Decrypted string as a string.</returns>
   Public Function DecryptDataAsString(stringToDecrypt As String) As String
      Return m_SplitKeyEncryption.DecryptDataAsString(stringToDecrypt)
   End Function

#End Region
#End Region

#Region "Private methods"
   '******************************************************************
   ' Private methods
   '******************************************************************

#Region "Check methods"
   ''' <summary>
   ''' Throws an exception if this instance is not in a valid state
   ''' </summary>
   ''' <exception cref="ObjectDisposedException">Thrown when this instance has already been disposed of.</exception>
   Private Sub CheckState()
      If m_IsDisposed Then _
         Throw New ObjectDisposedException(NameOf(FileAndKeyEncryption))
   End Sub
#End Region

#Region "File helper methods"
   ''' <summary>
   ''' Get the content of the key file.
   ''' </summary>
   ''' <param name="keyFilePath">Key file to be used.</param>
   ''' <returns>Content of the key file.</returns>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="keyFilePath"/> contains invalid characters.</exception>
   ''' <exception cref="DirectoryNotFoundException">Thrown if the directory in <paramref name="keyFilePath"/> does not exist.</exception>
   ''' <exception cref="FileNotFoundException">Thrown if <paramref name="keyFilePath"/> is not found.</exception>
   ''' <exception cref="IOException">Thrown if an I/O error occurs during access to the file.</exception>
   ''' <exception cref="NotSupportedException">Thrown if <paramref name="keyFilePath"/> is in an invalid format.</exception>
   ''' <exception cref="PathTooLongException">Thrown if <paramref name="keyFilePath"/> is too long.</exception>
   ''' <exception cref="UnauthorizedAccessException">Thrown if <paramref name="keyFilePath"/> specifies a directory or the caller
   ''' does not have permission to read the file.</exception>
   Private Shared Function GetContentOfFile(keyFilePath As String) As Byte()
      Dim result As Byte()

      If File.Exists(keyFilePath) Then
         result = File.ReadAllBytes(keyFilePath)
      Else
         Throw New FileNotFoundException("File '" & keyFilePath & "' does not exist")
      End If

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
            '
            ' Disposing of resources needs to be synchronized to prevent a race condition.
            '
            SyncLock m_SplitKeyEncryption
               m_SplitKeyEncryption.Dispose()
            End SyncLock
         End If

         ' Free unmanaged resources (unmanaged objects) and override Finalize() below.
         ' Set large fields to null.
      End If
   End Sub

   ' Override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
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
      ' Uncomment the following line if Finalize() is overridden above.
      ' GC.SuppressFinalize(Me)
   End Sub
#End Region
End Class
