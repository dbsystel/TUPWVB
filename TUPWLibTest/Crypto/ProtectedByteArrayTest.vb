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
' Version: 1.0.1
'
' Change history:
'    2020-04-29: V1.0.0: Created.
'    2020-12-11: V1.0.1: Check for corrected exception after Dispose.
'

Option Strict On
Option Explicit On

Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TUPWLib

<TestClass()> Public Class ProtectedByteArrayTest
#Region "Private constants"
   '
   ' Private constants
   '
   Private Const FILL_VALUE As Byte = &H55

   Private Const EXPECTED_EXCEPTION As String = "Expected exception not thrown"
#End Region

#Region "Test methods"
   <TestMethod()> Public Sub TestNullArgument()
      Try
#Disable Warning S1481 ' Unused local variables should be removed
         Dim pba As New ProtectedByteArray(Nothing)
#Enable Warning S1481 ' Unused local variables should be removed

         Assert.Fail(EXPECTED_EXCEPTION)

      Catch ex As ArgumentNullException
         Dim message As String = ex.Message()

         Assert.IsTrue(message.Contains("sourceArray"), message)

      Catch ex As Exception
         Assert.Fail("Exception: " & ex.Message() & " / " & ex.StackTrace())
      End Try
   End Sub

   <TestMethod()> Public Sub TestEmptyArgument()
      Dim pba As New ProtectedByteArray(Array.Empty(Of Byte)())

      Dim result As Byte() = pba.GetData()

      Assert.AreEqual(0, result.Length, "Empty byte array is retrieved with wrong length")
   End Sub

   <TestMethod()> Public Sub TestBase()
      Dim ba As Byte() = New Byte(31) {}

      ArrayHelper.Fill(ba, FILL_VALUE)

      Dim pba As New ProtectedByteArray(ba)

      Assert.IsTrue(ArrayHelper.AreEqual(ba, pba.GetData()), "Data was not correctly retrieved")
      Assert.AreEqual(ba.Length, pba.Length(), "Retrieved data has different length from stored data")
   End Sub

   <TestMethod()> Public Sub TestClose()
      Dim ba As Byte() = New Byte(31) {}

      ArrayHelper.Fill(ba, FILL_VALUE)

      Dim pba As New ProtectedByteArray(ba)

      pba.Dispose()

      Assert.IsFalse(pba.IsValid(), "ProtectedByteArray still valid after close")

      Try
         pba.GetData()

         Assert.Fail(EXPECTED_EXCEPTION)

      Catch ex As ObjectDisposedException
         '
         ' This is the expected exception
         '

      Catch ex As Exception
         Assert.Fail("Exception: " & ex.Message() & " / " & ex.StackTrace())
      End Try
   End Sub

   <TestMethod()> Public Sub TestEquals()
      Dim ba As Byte() = New Byte(31) {}

      ArrayHelper.Fill(ba, FILL_VALUE)

      Dim pba1 As New ProtectedByteArray(ba)
      Dim pba2 As New ProtectedByteArray(ba)

      Assert.AreEqual(pba1, pba2, "ProtectedByteArray are not equal when they should be")
      Assert.AreEqual(pba1.GetHashCode(), pba2.GetHashCode(), "ProtectedByteArray do not have identical hash codes")

      Dim pba3 As New ProtectedByteArray(New Byte(31) {})
      Assert.AreNotEqual(pba1, pba3, "ProtectedByteArray are equal when they should not be (different keys)")
   End Sub
#End Region
End Class