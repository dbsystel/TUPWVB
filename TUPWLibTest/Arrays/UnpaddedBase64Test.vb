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
' Version: 1.0.0
'
' Change history:
'    2020-04-29: V1.0.0: Created.
'

Option Strict On
Option Explicit On

Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TUPWLib

<TestClass()> Public Class UnpaddedBase64Test
#Region "Private constants"
   Private Const EXPECTED_EXCEPTION As String = "Expected exception not thrown"
#End Region

   <TestMethod()> Public Sub TestBasic()
      Dim rnd As New Random

      For i As Short = 1 To 200
         Dim arrayMaxIndex As Integer = rnd.Next(0, 199)
         Dim aByteArray As Byte() = New Byte(0 To arrayMaxIndex) {}
         rnd.NextBytes(aByteArray)

         Dim b64Text As String = UnpaddedBase64.ToUnpaddedBase64String(aByteArray)

         Dim reconstructedByteArray As Byte() = UnpaddedBase64.FromUnpaddedBase64String(b64Text)

         Assert.IsTrue(ArrayHelper.AreEqual(aByteArray, reconstructedByteArray), "Reconstructed byte array does not match original byte array")
      Next
   End Sub

   <TestMethod()> Public Sub TestBoundaries()
      Dim aByteArray As Byte() = Array.Empty(Of Byte)()
      Dim b64Text As String = UnpaddedBase64.ToUnpaddedBase64String(aByteArray)

      Dim reconstructedByteArray As Byte() = UnpaddedBase64.FromUnpaddedBase64String(b64Text)

      Assert.IsTrue(ArrayHelper.AreEqual(aByteArray, reconstructedByteArray), "Reconstructed empty byte array does not match original byte array")
   End Sub

   <TestMethod()> Public Sub TestShortBase64()
      Dim b64Text As String = "A"

      Try
#Disable Warning S1481 ' Unused local variables should be removed
         Dim reconstructedByteArray As Byte() = UnpaddedBase64.FromUnpaddedBase64String(b64Text)
#Enable Warning S1481 ' Unused local variables should be removed

         Assert.Fail(EXPECTED_EXCEPTION)

      Catch ex As FormatException
         '
         ' This exception is expected
         '

      Catch ex As Exception
         Assert.Fail("Exception: " & ex.Message() & " / " & ex.StackTrace())
      End Try
   End Sub

   <TestMethod()> Public Sub TestInvalidBase64Character()
      Dim b64Text As String = "RWluIFR$eHQ="

      Try
#Disable Warning S1481 ' Unused local variables should be removed
         Dim reconstructedByteArray As Byte() = UnpaddedBase64.FromUnpaddedBase64String(b64Text)
#Enable Warning S1481 ' Unused local variables should be removed

         Assert.Fail(EXPECTED_EXCEPTION)

      Catch ex As FormatException
         '
         ' This exception is expected
         '

      Catch ex As Exception
         Assert.Fail("Exception: " & ex.Message() & " / " & ex.StackTrace())
      End Try
   End Sub

   <TestMethod()> Public Sub TestInvalidBase64Padding()
      Dim b64Text As String = "RWluIFRleHQ==="

      Try
#Disable Warning S1481 ' Unused local variables should be removed
         Dim reconstructedByteArray As Byte() = UnpaddedBase64.FromUnpaddedBase64String(b64Text)
#Enable Warning S1481 ' Unused local variables should be removed

         Assert.Fail(EXPECTED_EXCEPTION)

      Catch ex As FormatException
         '
         ' This exception is expected
         '

      Catch ex As Exception
         Assert.Fail("Exception: " & ex.Message() & " / " & ex.StackTrace())
      End Try
   End Sub
End Class
