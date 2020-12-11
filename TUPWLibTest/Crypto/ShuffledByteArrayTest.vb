'
' SPDX-FileCopyrightText: 2020 DB Systel GmbH
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
'    2020-04-21: V1.0.0: Created.
'    2020-12-11: V1.0.1: Check for corrected exception after Dispose.
'

'Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports DB.BCM.TUPW

<TestClass()> Public Class ShuffledByteArrayTest
#Region "Private constants"
   '''
   ''' Private constants
   '''
   Private Const FILL_VALUE As Byte = &H55
   Private Const OTHER_VALUE As Byte = &HAA
   Private Const CHANGE_INDEX As Integer = 7

   Private Const EXPECTED_EXCEPTION As String = "Expected exception not thrown"
#End Region

   <TestMethod()> Public Sub TestNullArgument()
      Dim sba As ShuffledByteArray = Nothing

      Try
         sba = New ShuffledByteArray(Nothing)

         If sba IsNot Nothing Then _
            sba.Dispose()

         Assert.Fail(EXPECTED_EXCEPTION)

      Catch ex As ArgumentNullException
         If sba IsNot Nothing Then _
            sba.Dispose()

         Dim message As String = ex.Message()

         Assert.IsTrue(message.Contains("sourceArray"), "Exception: " & message)

      Catch ex As Exception
         If sba IsNot Nothing Then _
            sba.Dispose()

         Assert.Fail("Exception: " & ex.Message() & " / " & ex.StackTrace())
      End Try
   End Sub

   <TestMethod()> Public Sub TestEmptyArgument()
      Using sba As ShuffledByteArray = New ShuffledByteArray(Array.Empty(Of Byte)())
         Dim result As Byte() = sba.GetData()

         Assert.AreEqual(0, result.Length, "Empty byte array is retrieved with wrong length")

         Try
#Disable Warning S1481 ' Unused local variables should be removed
            Dim nonExistent As Byte = sba.ElementAt(0)
#Enable Warning S1481 ' Unused local variables should be removed

            Assert.Fail(EXPECTED_EXCEPTION)

         Catch ex As IndexOutOfRangeException
            '
            ' This exception is expected and so we just continue here
            '

         Catch ex As Exception
            Assert.Fail("Exception: " & ex.Message() & " / " & ex.StackTrace())
         End Try
      End Using
   End Sub

   <TestMethod()> Public Sub TestBase()
      Dim ba As Byte() = New Byte(0 To 31) {}

      ArrayHelper.Fill(ba, FILL_VALUE)

      Using sba As ShuffledByteArray = New ShuffledByteArray(ba)
         Assert.IsTrue(ArrayHelper.AreEqual(ba, sba.GetData()), "Data was not correctly retrieved")
         Assert.AreEqual(ba.Length, sba.Length, "Retrieved data has different length from stored data")
         Assert.AreEqual(ba(0), sba.ElementAt(0), "Retrieved data at index 0 has different value from stored data")

         sba.ElementAt(CHANGE_INDEX) = OTHER_VALUE

         Assert.AreEqual(OTHER_VALUE, sba.ElementAt(CHANGE_INDEX), "Retrieved data with 'getAt' has different value from what was set")

         Dim retrievedBa As Byte() = sba.GetData()

         Assert.AreEqual(OTHER_VALUE, retrievedBa(CHANGE_INDEX), "Retrieved data with 'getData' has different value from what was set")
      End Using
   End Sub

   <TestMethod()> Public Sub TestDispose()
      Dim ba As Byte() = New Byte(0 To 31) {}

      ArrayHelper.Fill(ba, FILL_VALUE)

      Dim sba As ShuffledByteArray = New ShuffledByteArray(ba)

      sba.Dispose()

      Assert.IsFalse(sba.IsValid, "ShuffledByteArray still valid after dispose")

      Try
         sba.getData()

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
      Dim ba As Byte() = New Byte(0 To 31) {}

      ArrayHelper.Fill(ba, FILL_VALUE)

      Using sba1 As ShuffledByteArray = New ShuffledByteArray(ba)
         Using sba2 As ShuffledByteArray = New ShuffledByteArray(ba)

            Assert.IsTrue(sba1.Equals(sba2), "ShuffledByteArray are not equal when they should be")
            Assert.AreEqual(sba1.GetHashCode(), sba2.GetHashCode(), "ShuffledByteArray hashes are not equal when they should be")

            Using sba3 As ShuffledByteArray = New ShuffledByteArray(New Byte(0 To 31) {})

               Assert.IsFalse(sba1.Equals(sba3), "ShuffledByteArray are equal when they should not be (different data)")
            End Using
         End Using
      End Using
   End Sub
End Class