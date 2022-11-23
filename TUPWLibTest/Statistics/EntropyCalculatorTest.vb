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
'    2020-05-12: V1.0.0: Created.
'

Option Strict On
Option Explicit On

Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TUPWLib

<TestClass()> Public Class EntropyCalculatorTest
#Region "Private constants"
   Private Const TEXT_EXPECTED_EXCEPTION As String = "Expected exception not thrown"
   Private Const TEXT_UNEXPECTED_EXCEPTION As String = "Unexpected exception: "
   Private Const TEXT_SEPARATOR As String = " / "
#End Region

#Region "Test methods"
   <TestMethod()> Public Sub TestRandom()
      Dim rd As New Random()

      Dim randomByteArray As Byte() = New Byte(0 To 9999) {}

      rd.NextBytes(randomByteArray)

      Dim ec As New EntropyCalculator()

      ec.AddBytes(randomByteArray)

      Assert.IsTrue(ec.GetEntropy() > 7.5, "Entropy of random array is too small")
      Assert.IsTrue(ec.GetInformationInBits() > 7500UI, "Information of random array is too small")
      Assert.IsTrue(ec.GetRelativeEntropy() > 0.95, "Relative entropy of random array is too small")
   End Sub

   <TestMethod()> Public Sub TestNoEntropy()
      Dim dullByteArray As Byte() = New Byte(0 To 9999) {}

      ArrayHelper.Fill(dullByteArray, &H44)

      Dim ec As New EntropyCalculator()

      ec.AddBytes(dullByteArray)

      Assert.AreEqual(0.0, ec.GetEntropy(), "Entropy of all-equal array is not 0")
      Assert.AreEqual(0UI, ec.GetInformationInBits(), "Information of all-equal array is not 0")
      Assert.AreEqual(0.0, ec.GetRelativeEntropy(), "Relative entropy of all-equal array is not 0")
   End Sub

   <TestMethod()> Public Sub TestNullByteArray()
      Dim ec As New EntropyCalculator()

      Try
         ec.AddBytes(Nothing)

         Assert.Fail(TEXT_EXPECTED_EXCEPTION)

      Catch ex As ArgumentNullException
         Dim message As String = ex.Message()

         Assert.IsTrue(message.Contains("aByteArray"), GetUnexpectedExceptionMessage(ex))

      Catch ex As Exception
         Assert.Fail(GetUnexpectedExceptionMessage(ex))
      End Try

   End Sub

   <TestMethod()> Public Sub TestZeroLengthByteArray()
      Dim emptyByteArray As Byte() = Array.Empty(Of Byte)()

      Dim ec As New EntropyCalculator()

      ec.AddBytes(emptyByteArray)

      Assert.AreEqual(0.0, ec.GetEntropy(), "Entropy of all-equal array is not 0")
      Assert.AreEqual(0UI, ec.GetInformationInBits(), "Information of all-equal array is not 0")
      Assert.AreEqual(0.0, ec.GetRelativeEntropy(), "Relative entropy of all-equal array is not 0")
   End Sub
#End Region

#Region "Private methods"
   ''' <summary>
   ''' Get text for unexpected exceptions.
   ''' </summary>
   ''' <param name="ex">The unexpected exception</param>
   ''' <returns>Text that describes the unexpected exception.</returns>
   Private Shared Function GetUnexpectedExceptionMessage(ex As Exception) As String
      Return TEXT_UNEXPECTED_EXCEPTION & ex.ToString() & TEXT_SEPARATOR & ex.Message() & TEXT_SEPARATOR & ex.StackTrace()
   End Function
#End Region

End Class