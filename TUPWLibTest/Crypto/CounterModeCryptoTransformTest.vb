'
' SPDX-FileCopyrightText: 2022 DB Systel GmbH
'
' SPDX-License-Identifier: Apache-2.0
'
' Licensed under the Apache License, Version 2.0 (the "License");
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
'    2020-05-12: V1.0.0: Created.
'    2020-10-26: V1.0.1: Fixed "TestInvalidAlgorithmName" to be more understandable.
'    2021-07-21: V1.0.2: Simplified constants instantiations.
'    2022-11-22: V1.1.0: Use Aes.Create() for cipher creation.
'

Option Strict On
Option Explicit On

Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System.IO
Imports System.Security.Cryptography
Imports TUPWLib

<TestClass()> Public Class CounterModeCryptoTransformTest
#Region "Private constants"
#Region "Exception texts"
   Private Const TEXT_EXPECTED_EXCEPTION As String = "Expected exception not thrown"
   Private Const TEXT_UNEXPECTED_EXCEPTION As String = "Unexpected exception: "
   Private Const TEXT_SEPARATOR As String = " / "
#End Region

#Region "Test vectors"
   Private Shared ReadOnly NIST_800_38A_IV As Byte() = {
      &HF0, &HF1, &HF2, &HF3,
      &HF4, &HF5, &HF6, &HF7,
      &HF8, &HF9, &HFA, &HFB,
      &HFC, &HFD, &HFE, &HFF
   }

   Private Shared ReadOnly NIST_800_38A_PLAIN_TEXT As Byte() = {
      &H6B, &HC1, &HBE, &HE2,
      &H2E, &H40, &H9F, &H96,
      &HE9, &H3D, &H7E, &H11,
      &H73, &H93, &H17, &H2A,
      &HAE, &H2D, &H8A, &H57,
      &H1E, &H3, &HAC, &H9C,
      &H9E, &HB7, &H6F, &HAC,
      &H45, &HAF, &H8E, &H51,
      &H30, &HC8, &H1C, &H46,
      &HA3, &H5C, &HE4, &H11,
      &HE5, &HFB, &HC1, &H19,
      &H1A, &HA, &H52, &HEF,
      &HF6, &H9F, &H24, &H45,
      &HDF, &H4F, &H9B, &H17,
      &HAD, &H2B, &H41, &H7B,
      &HE6, &H6C, &H37, &H10
   }

   Private Shared ReadOnly NIST_800_38A_128_BIT_KEY As Byte() = {
      &H2B, &H7E, &H15, &H16,
      &H28, &HAE, &HD2, &HA6,
      &HAB, &HF7, &H15, &H88,
      &H9, &HCF, &H4F, &H3C
   }

   Private Shared ReadOnly NIST_800_38A_128_BIT_CIPHER_TEXT As Byte() = {
      &H87, &H4D, &H61, &H91,
      &HB6, &H20, &HE3, &H26,
      &H1B, &HEF, &H68, &H64,
      &H99, &HD, &HB6, &HCE,
      &H98, &H6, &HF6, &H6B,
      &H79, &H70, &HFD, &HFF,
      &H86, &H17, &H18, &H7B,
      &HB9, &HFF, &HFD, &HFF,
      &H5A, &HE4, &HDF, &H3E,
      &HDB, &HD5, &HD3, &H5E,
      &H5B, &H4F, &H9, &H2,
      &HD, &HB0, &H3E, &HAB,
      &H1E, &H3, &H1D, &HDA,
      &H2F, &HBE, &H3, &HD1,
      &H79, &H21, &H70, &HA0,
      &HF3, &H0, &H9C, &HEE
   }

   Private Shared ReadOnly NIST_800_38A_192_BIT_KEY As Byte() = {
      &H8E, &H73, &HB0, &HF7,
      &HDA, &HE, &H64, &H52,
      &HC8, &H10, &HF3, &H2B,
      &H80, &H90, &H79, &HE5,
      &H62, &HF8, &HEA, &HD2,
      &H52, &H2C, &H6B, &H7B
   }

   Private Shared ReadOnly NIST_800_38A_192_BIT_CIPHER_TEXT As Byte() = {
      &H1A, &HBC, &H93, &H24,
      &H17, &H52, &H1C, &HA2,
      &H4F, &H2B, &H4, &H59,
      &HFE, &H7E, &H6E, &HB,
      &H9, &H3, &H39, &HEC,
      &HA, &HA6, &HFA, &HEF,
      &HD5, &HCC, &HC2, &HC6,
      &HF4, &HCE, &H8E, &H94,
      &H1E, &H36, &HB2, &H6B,
      &HD1, &HEB, &HC6, &H70,
      &HD1, &HBD, &H1D, &H66,
      &H56, &H20, &HAB, &HF7,
      &H4F, &H78, &HA7, &HF6,
      &HD2, &H98, &H9, &H58,
      &H5A, &H97, &HDA, &HEC,
      &H58, &HC6, &HB0, &H50
   }

   Private Shared ReadOnly NIST_800_38A_256_BIT_KEY As Byte() = {
      &H60, &H3D, &HEB, &H10,
      &H15, &HCA, &H71, &HBE,
      &H2B, &H73, &HAE, &HF0,
      &H85, &H7D, &H77, &H81,
      &H1F, &H35, &H2C, &H7,
      &H3B, &H61, &H8, &HD7,
      &H2D, &H98, &H10, &HA3,
      &H9, &H14, &HDF, &HF4
   }

   Private Shared ReadOnly NIST_800_38A_256_BIT_CIPHER_TEXT As Byte() = {
      &H60, &H1E, &HC3, &H13,
      &H77, &H57, &H89, &HA5,
      &HB7, &HA7, &HF5, &H4,
      &HBB, &HF3, &HD2, &H28,
      &HF4, &H43, &HE3, &HCA,
      &H4D, &H62, &HB5, &H9A,
      &HCA, &H84, &HE9, &H90,
      &HCA, &HCA, &HF5, &HC5,
      &H2B, &H9, &H30, &HDA,
      &HA2, &H3D, &HE9, &H4C,
      &HE8, &H70, &H17, &HBA,
      &H2D, &H84, &H98, &H8D,
      &HDF, &HC9, &HC5, &H8D,
      &HB6, &H7A, &HAD, &HA6,
      &H13, &HC2, &HDD, &H8,
      &H45, &H79, &H41, &HA6
   }
#End Region
#End Region

#Region "Test methods"
   <TestMethod()> Public Sub TestAES128Bit()
      Dim encryptedBytes As Byte() = DoEncryption(NIST_800_38A_128_BIT_KEY, NIST_800_38A_IV, NIST_800_38A_PLAIN_TEXT)

      Assert.IsTrue(ArrayHelper.AreEqual(NIST_800_38A_128_BIT_CIPHER_TEXT, encryptedBytes), "Cipher text does not have the expected value")

      Dim decryptedBytes As Byte() = DoEncryption(NIST_800_38A_128_BIT_KEY, NIST_800_38A_IV, encryptedBytes)

      Assert.IsTrue(ArrayHelper.AreEqual(NIST_800_38A_PLAIN_TEXT, decryptedBytes), "Cipher text not correctly decrypted")
   End Sub

   <TestMethod()> Public Sub TestAES192Bit()
      Dim encryptedBytes As Byte() = DoEncryption(NIST_800_38A_192_BIT_KEY, NIST_800_38A_IV, NIST_800_38A_PLAIN_TEXT)

      Assert.IsTrue(ArrayHelper.AreEqual(NIST_800_38A_192_BIT_CIPHER_TEXT, encryptedBytes), "Cipher text does not have the expected value")

      Dim decryptedBytes As Byte() = DoEncryption(NIST_800_38A_192_BIT_KEY, NIST_800_38A_IV, encryptedBytes)

      Assert.IsTrue(ArrayHelper.AreEqual(NIST_800_38A_PLAIN_TEXT, decryptedBytes), "Cipher text not correctly decrypted")
   End Sub

   <TestMethod()> Public Sub TestAES256Bit()
      Dim encryptedBytes As Byte() = DoEncryption(NIST_800_38A_256_BIT_KEY, NIST_800_38A_IV, NIST_800_38A_PLAIN_TEXT)

      Assert.IsTrue(ArrayHelper.AreEqual(NIST_800_38A_256_BIT_CIPHER_TEXT, encryptedBytes), "Cipher text does not have the expected value")

      Dim decryptedBytes As Byte() = DoEncryption(NIST_800_38A_256_BIT_KEY, NIST_800_38A_IV, encryptedBytes)

      Assert.IsTrue(ArrayHelper.AreEqual(NIST_800_38A_PLAIN_TEXT, decryptedBytes), "Cipher text not correctly decrypted")
   End Sub

   <TestMethod()> Public Sub TestAlgorithmName()
      Dim encryptedBytes As Byte()

      Using ms As New MemoryStream()
         Using cmct As New CounterModeCryptoTransform("Aes", NIST_800_38A_128_BIT_KEY, NIST_800_38A_IV)
            Using encryptionStream As New CryptoStream(ms, cmct, CryptoStreamMode.Write)
               encryptionStream.Write(NIST_800_38A_PLAIN_TEXT, 0, NIST_800_38A_PLAIN_TEXT.Length)

               encryptionStream.FlushFinalBlock()
            End Using
         End Using

         encryptedBytes = ms.ToArray()
      End Using

      Assert.IsTrue(ArrayHelper.AreEqual(NIST_800_38A_128_BIT_CIPHER_TEXT, encryptedBytes), "Cipher text does not have the expected value")
   End Sub

   <TestMethod()> Public Sub TestNullKey()
      Try
#Disable Warning S1481 ' Unused local variables should be removed
         Dim encryptedBytes As Byte() = DoEncryption(Nothing, NIST_800_38A_IV, NIST_800_38A_PLAIN_TEXT)
#Enable Warning S1481 ' Unused local variables should be removed

         Assert.Fail(TEXT_EXPECTED_EXCEPTION)

      Catch ex As ArgumentNullException
         '
         ' Expected exception
         '
      Catch ex As Exception
         Assert.Fail(GetUnexpectedExceptionMessage(ex))
      End Try
   End Sub
   <TestMethod()> Public Sub TestShortKey()
      Dim shortKey As Byte() = New Byte() {&HAA, &HBB, &HCC}

      Try
#Disable Warning S1481 ' Unused local variables should be removed
         Dim encryptedBytes As Byte() = DoEncryption(shortKey, NIST_800_38A_IV, NIST_800_38A_PLAIN_TEXT)
#Enable Warning S1481 ' Unused local variables should be removed

         Assert.Fail(TEXT_EXPECTED_EXCEPTION)

      Catch ex As ArgumentException
         '
         ' Expected exception
         '
      Catch ex As Exception
         Assert.Fail(GetUnexpectedExceptionMessage(ex))
      End Try
   End Sub

   <TestMethod()> Public Sub TestNullIV()
      Try
#Disable Warning S1481 ' Unused local variables should be removed
         Dim encryptedBytes As Byte() = DoEncryption(NIST_800_38A_128_BIT_KEY, Nothing, NIST_800_38A_PLAIN_TEXT)
#Enable Warning S1481 ' Unused local variables should be removed

         Assert.Fail(TEXT_EXPECTED_EXCEPTION)

      Catch ex As ArgumentNullException
         '
         ' Expected exception
         '
      Catch ex As Exception
         Assert.Fail(GetUnexpectedExceptionMessage(ex))
      End Try
   End Sub

   <TestMethod()> Public Sub TestShortIV()
      Dim shortIV As Byte() = New Byte() {&HAA, &HBB, &HCC}

      Try
#Disable Warning S1481 ' Unused local variables should be removed
         Dim encryptedBytes As Byte() = DoEncryption(NIST_800_38A_128_BIT_KEY, shortIV, NIST_800_38A_PLAIN_TEXT)
#Enable Warning S1481 ' Unused local variables should be removed

         Assert.Fail(TEXT_EXPECTED_EXCEPTION)

      Catch ex As ArgumentException
         '
         ' Expected exception
         '
      Catch ex As Exception
         Assert.Fail(GetUnexpectedExceptionMessage(ex))
      End Try
   End Sub

   <TestMethod()> Public Sub TestInvalidAlgorithmName()
      Try
         Using ct As ICryptoTransform = New CounterModeCryptoTransform("xazq", NIST_800_38A_128_BIT_KEY, NIST_800_38A_IV)
            Assert.Fail(TEXT_EXPECTED_EXCEPTION)
         End Using

      Catch ex As ArgumentException
         '
         ' Expected exception
         '
         Assert.IsTrue(ex.Message().Contains("algorithmName"), GetUnexpectedExceptionMessage(ex))

      Catch ex As Exception
         Assert.Fail(GetUnexpectedExceptionMessage(ex))
      End Try
   End Sub
#End Region

#Region "Private methods"
#Region "Encryption helpers"
   ''' <summary>
   ''' Actually perform the encryption and decryption.
   ''' </summary>
   ''' <remarks>
   ''' Encryption and decryption is the same operation with counter mode.
   ''' </remarks> 
   ''' <param name="key">Key to use.</param>
   ''' <param name="iv">Initialization vector to use.</param>
   ''' <param name="plaintext">Plain text to encrypt or decrypt.</param>
   ''' <returns>Encrypted or decrypted text.</returns>
   Private Function DoEncryption(key As Byte(), iv As Byte(), plaintext As Byte()) As Byte()
      Dim result As Byte()

      Using algo As Aes = Aes.Create()
         Using ms As New MemoryStream()
            Using cmct As New CounterModeCryptoTransform(algo, key, iv)
               Using encryptionStream As New CryptoStream(ms, cmct, CryptoStreamMode.Write)
                  encryptionStream.Write(plaintext, 0, plaintext.Length)

                  encryptionStream.FlushFinalBlock()
               End Using
            End Using

            result = ms.ToArray()
         End Using
      End Using

      Return result
   End Function
#End Region

#Region "Exception helpers"
   ''' <summary>
   ''' Get text for unexpected exceptions.
   ''' </summary>
   ''' <param name="ex">The unexpected exception</param>
   ''' <returns>Text that describes the unexpected exception.</returns>
   Private Function GetUnexpectedExceptionMessage(ex As Exception) As String
      Return TEXT_UNEXPECTED_EXCEPTION & ex.ToString() & TEXT_SEPARATOR & ex.Message() & TEXT_SEPARATOR & ex.StackTrace()
   End Function
#End Region
#End Region
End Class