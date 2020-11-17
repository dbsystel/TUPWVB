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
' Version: 1.1.0
'
' Change history:
'    2020-05-05: V1.0.0: Created.
'    2020-05-14: V1.1.0: Correct usage of Dispose interface.
'

Imports System.IO
'Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports DB.BCM.TUPW

<TestClass()> Public Class FileAndKeyEncryptionTest
   ''' <summary>
   ''' File name for the non-random bytes.
   ''' </summary>
   Private Const NOT_RANDOM_FILE_NAME As String = "_not_random_file_.bin"

   ''' <summary>
   ''' HMAC key to be used for encryption.
   ''' </summary>
   Private Shared ReadOnly HMAC_KEY As Byte() = {&HC1, &HC2, &HC8, &HF,
      &HDE, &H75, &HD7, &HA9,
      &HFC, &H92, &H56, &HEA,
      &H3C, &HC, &H7A, &H8,
      &H8A, &H6E, &HB5, &H78,
      &H15, &H79, &HCF, &HB4,
      &H2, &HF, &H38, &H3C,
      &H61, &H4F, &H9D, &HDB}

   ''' <summary>
   ''' Known clear text to encrypt.
   ''' </summary>
   Private Const CLEAR_TEXT_V5 As String = "This#" & ChrW(&H201D) & "s?a§StR4nGé€PàS!Wörd9"

   ''' <summary>
   ''' Subject for known encryption.
   ''' </summary>
   Private Const SUBJECT As String = "strangeness+charm"

   '
   ' Error message constants
   '
   Private Const TEXT_EXPECTED_EXCEPTION As String = "Expected exception Not thrown"
   Private Const TEXT_UNEXPECTED_EXCEPTION As String = "Unexpected exception "
   Private Const TEXT_SEPARATOR As String = " / "

   ''' <summary>
   ''' Create test file for all the tests.
   ''' </summary>
   ''' <param name="tc">Test context to use (not used here).</param>
#Disable Warning IDE0060
   <ClassInitialize()> Public Shared Sub InitializeTests(tc As TestContext)
#Enable Warning IDE0060
      '
      ' Generate a nonrandom key file with a predictable content, so the tests are reproducible.
      '
      Dim notRandomBytes As Byte() = New Byte(0 To 99999) {}

      For i As Integer = 0 To notRandomBytes.Length - 1
         notRandomBytes(i) = CByte(&HFF - (i And &HFF))
      Next

      Dim filePath As String = Path.GetFullPath(NOT_RANDOM_FILE_NAME)

      Try
         File.WriteAllBytes(filePath, notRandomBytes)

      Catch ex As Exception
         Assert.Fail("Could Not write to file '" & NOT_RANDOM_FILE_NAME & ": " & ex.Message())
      End Try
   End Sub

   ''' <summary>
   ''' Remove test file after all tests have been executed.
   ''' </summary>
   <ClassCleanup()> Public Shared Sub CleanupTests()
      Dim filePath As String = Path.GetFullPath(NOT_RANDOM_FILE_NAME)

      Try
         If File.Exists(filePath) Then _
            File.Delete(filePath)

      Catch ex As Exception
         Assert.Fail("Could not delete file '" & NOT_RANDOM_FILE_NAME & ": " & ex.Message())
      End Try
   End Sub

#Region "Test methods"
   ''' <summary>
   ''' Test if the encryption of a given byte array is correctly decrypted.
   ''' </summary>
   <TestMethod()> Public Sub TestEncryptionDecryptionForByteArray()
      Dim myEncryptor As FileAndKeyEncryption = Nothing

      Try
         Dim testByteArray As Byte() = New Byte(0 To 255) {}

         For i As Integer = 0 To testByteArray.Length - 1
            testByteArray(i) = CByte(&HFF - i)
         Next

         myEncryptor = New FileAndKeyEncryption(HMAC_KEY, NOT_RANDOM_FILE_NAME)

         Dim encryptedText As String = myEncryptor.EncryptData(testByteArray)

         Dim decryptedByteArray As Byte() = myEncryptor.DecryptDataAsByteArray(encryptedText)

         Assert.IsTrue(ArrayHelper.AreEqual(testByteArray, decryptedByteArray), "Decrypted byte array is not the same as original byte array")

         myEncryptor.Dispose()

      Catch ex As Exception
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

         Assert.Fail(GetUnexpectedExceptionMessage(ex))
      End Try
   End Sub

   ''' <summary>
   ''' Test if the encryption of a given character array is correctly decrypted.
   ''' </summary>
   <TestMethod()> Public Sub TestEncryptionDecryptionForCharacterArray()
      Dim myEncryptor As FileAndKeyEncryption = Nothing

      Try
         Dim testCharArray As Char() = New Char() {"T"c, "h"c, "í"c, "s"c, " "c, "ì"c, "s"c, " "c, "a"c, " "c, "T"c, "ä"c, "s"c, "t"c}

         myEncryptor = New FileAndKeyEncryption(HMAC_KEY, NOT_RANDOM_FILE_NAME)

         Dim encryptedText As String = myEncryptor.EncryptData(testCharArray)

         Dim decryptedCharArray As Char() = myEncryptor.DecryptDataAsCharacterArray(encryptedText)

         myEncryptor.Dispose()

         Assert.IsTrue(ArrayHelper.AreEqual(testCharArray, decryptedCharArray), "Decrypted character array is not the same as original character array")

      Catch ex As Exception
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

         Assert.Fail(GetUnexpectedExceptionMessage(ex))
      End Try
   End Sub

   ''' <summary>
   ''' Test if the encryption of a given text is correctly decrypted.
   ''' </summary>
   <TestMethod()> Public Sub TestEncryptionDecryption()
      Dim myEncryptor As FileAndKeyEncryption = Nothing

      Try
         myEncryptor = New FileAndKeyEncryption(HMAC_KEY, NOT_RANDOM_FILE_NAME)

         Dim encryptedText As String = myEncryptor.EncryptData(CLEAR_TEXT_V5)

         Dim decryptedText As String = myEncryptor.DecryptDataAsString(encryptedText)

         Assert.AreEqual(CLEAR_TEXT_V5, decryptedText, "Decrypted text is not the same as original text")

         myEncryptor.Dispose()

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
      Dim myEncryptor As FileAndKeyEncryption = Nothing

      Try
         myEncryptor = New FileAndKeyEncryption(HMAC_KEY, NOT_RANDOM_FILE_NAME)

         Dim encryptedText As String = myEncryptor.EncryptData(CLEAR_TEXT_V5, SUBJECT)

         Dim decryptedText As String = myEncryptor.DecryptDataAsString(encryptedText, SUBJECT)

         Assert.AreEqual(CLEAR_TEXT_V5, decryptedText, "Decrypted text is not the same as original text")

         myEncryptor.Dispose()

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
      Dim myEncryptor As FileAndKeyEncryption = Nothing

      Try
         Dim testByteArray As Byte() = New Byte(0 To 255) {}

         For i As Integer = 0 To testByteArray.Length - 1
            testByteArray(i) = CByte(&HFF - i)
         Next

         myEncryptor = New FileAndKeyEncryption(HMAC_KEY, NOT_RANDOM_FILE_NAME)

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
      Dim myEncryptor As FileAndKeyEncryption = Nothing

      Try
         Dim testByteArray As Byte() = New Byte(0 To 255) {}

         For i As Integer = 0 To testByteArray.Length - 1
            testByteArray(i) = CByte(&HFF - i)
         Next

         myEncryptor = New FileAndKeyEncryption(HMAC_KEY, NOT_RANDOM_FILE_NAME)

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
   ''' Test if a file that does not exist is correctly handled.
   ''' </summary>
   <TestMethod()> Public Sub TestFileDoesNotExist()
      Dim myEncryptor As FileAndKeyEncryption = Nothing

      Try
         myEncryptor = New FileAndKeyEncryption(HMAC_KEY, "/does/not/exist.txt")

#Disable Warning S1481 ' Unused local variables should be removed
         Dim encryptedText As String = myEncryptor.EncryptData(CLEAR_TEXT_V5)
#Enable Warning S1481 ' Unused local variables should be removed

         myEncryptor.Dispose()

         Assert.Fail(TEXT_EXPECTED_EXCEPTION)

      Catch ex As FileNotFoundException
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

         Dim exceptionMessage As String = ex.Message()

         Assert.IsTrue(exceptionMessage.Contains("does not exist"), GetUnexpectedExceptionMessage(ex))

      Catch ex As Exception
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

         Assert.Fail(GetUnexpectedExceptionMessage(ex))
      End Try
   End Sub

   ''' <summary>
   ''' Test if invalid file name throws an exception.
   ''' </summary>
   <TestMethod()> Public Sub TestFileNameWithInvalidCharacters()
      Dim myEncryptor As FileAndKeyEncryption = Nothing

      Try
         myEncryptor = New FileAndKeyEncryption(HMAC_KEY, "|<>&")

#Disable Warning S1481 ' Unused local variables should be removed
         Dim encryptedText As String = myEncryptor.EncryptData(CLEAR_TEXT_V5)
#Enable Warning S1481 ' Unused local variables should be removed

         myEncryptor.Dispose()

         Assert.Fail(TEXT_EXPECTED_EXCEPTION)

      Catch ex As FileNotFoundException
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

         Dim exceptionMessage As String = ex.Message()

         Assert.IsTrue(exceptionMessage.Contains("does not exist"), GetUnexpectedExceptionMessage(ex))

      Catch ex As Exception
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

         Assert.Fail(GetUnexpectedExceptionMessage(ex))
      End Try
   End Sub

   ''' <summary>
   ''' Test if null file name throws an exception.
   ''' </summary>
   <TestMethod()> Public Sub TestNullFileName()
      Dim myEncryptor As FileAndKeyEncryption = Nothing

      Try
         myEncryptor = New FileAndKeyEncryption(HMAC_KEY, Nothing)

#Disable Warning S1481 ' Unused local variables should be removed
         Dim encryptedText As String = myEncryptor.EncryptData(CLEAR_TEXT_V5)
#Enable Warning S1481 ' Unused local variables should be removed

         myEncryptor.Dispose()

         Assert.Fail(TEXT_EXPECTED_EXCEPTION)

      Catch ex As ArgumentNullException
         If myEncryptor IsNot Nothing Then _
            myEncryptor.Dispose()

         Dim exceptionMessage As String = ex.Message()

         Assert.IsTrue(exceptionMessage.Contains("keyFilePath"), GetUnexpectedExceptionMessage(ex))

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