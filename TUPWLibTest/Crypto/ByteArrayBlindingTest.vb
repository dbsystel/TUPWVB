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
'    2020-04-27: V1.0.0: Created.
'

Option Strict On
Option Explicit On

Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TUPWLib

<TestClass()> Public Class ByteArrayBlindingTest
#Region "Private constants"
   Private Const ERROR_MESSAGE_BLINDED_DATA_TOO_SHORT As String = "Blinded data not longer than source data"
   Private Const ERROR_MESSAGE_LENGTH_MISMATCH As String = "Lengths are not the same after blinding and unblinding"
   Private Const ERROR_MESSAGE_DATA_MISMATCH As String = "Data is not the same after blinding and unblinding"
#End Region
   ''' <summary>
   ''' Test cases for ByteArrayBlinding.
   ''' </summary>
   <TestMethod()> Public Sub TestBlinding()
      Dim rng As New Random()

      Dim data0 As Byte() = Array.Empty(Of Byte)()

      Dim blindedData As Byte() = ByteArrayBlinding.BuildBlindedByteArray(data0, 17)
      Dim unblindedData As Byte() = ByteArrayBlinding.UnBlindByteArray(blindedData)

      Assert.IsTrue(blindedData.Length > data0.Length, ERROR_MESSAGE_BLINDED_DATA_TOO_SHORT)
      Assert.AreEqual(data0.Length, unblindedData.Length, ERROR_MESSAGE_LENGTH_MISMATCH)
      Assert.IsTrue(ArrayHelper.AreEqual(data0, unblindedData), ERROR_MESSAGE_DATA_MISMATCH)

      Dim data1 As Byte() = New Byte(0 To 0) {}
      rng.NextBytes(data1)

      blindedData = ByteArrayBlinding.BuildBlindedByteArray(data1, 17)
      unblindedData = ByteArrayBlinding.UnBlindByteArray(blindedData)

      Assert.IsTrue(blindedData.Length > data1.Length, ERROR_MESSAGE_BLINDED_DATA_TOO_SHORT)
      Assert.AreEqual(data1.Length, unblindedData.Length, ERROR_MESSAGE_LENGTH_MISMATCH)
      Assert.IsTrue(ArrayHelper.AreEqual(data1, unblindedData), ERROR_MESSAGE_DATA_MISMATCH)

      Dim data2 As Byte() = New Byte(0 To 15) {}
      rng.NextBytes(data2)

      blindedData = ByteArrayBlinding.BuildBlindedByteArray(data2, 17)
      unblindedData = ByteArrayBlinding.UnBlindByteArray(blindedData)

      Assert.IsTrue(blindedData.Length > data2.Length, ERROR_MESSAGE_BLINDED_DATA_TOO_SHORT)
      Assert.AreEqual(data2.Length, unblindedData.Length, ERROR_MESSAGE_LENGTH_MISMATCH)
      Assert.IsTrue(ArrayHelper.AreEqual(data2, unblindedData), ERROR_MESSAGE_DATA_MISMATCH)

      Dim data3 As Byte() = New Byte(0 To 19) {}
      rng.NextBytes(data3)

      blindedData = ByteArrayBlinding.BuildBlindedByteArray(data3, 17)
      unblindedData = ByteArrayBlinding.UnBlindByteArray(blindedData)

      Assert.IsTrue(blindedData.Length > data3.Length, ERROR_MESSAGE_BLINDED_DATA_TOO_SHORT)
      Assert.AreEqual(data3.Length, unblindedData.Length, ERROR_MESSAGE_LENGTH_MISMATCH)
      Assert.IsTrue(ArrayHelper.AreEqual(data3, unblindedData), ERROR_MESSAGE_DATA_MISMATCH)

      Dim data4 As Byte() = New Byte(0 To 17999) {}
      rng.NextBytes(data4)

      blindedData = ByteArrayBlinding.BuildBlindedByteArray(data4, 17)
      unblindedData = ByteArrayBlinding.UnBlindByteArray(blindedData)

      Assert.IsTrue(blindedData.Length > data4.Length, ERROR_MESSAGE_BLINDED_DATA_TOO_SHORT)
      Assert.AreEqual(data4.Length, unblindedData.Length, ERROR_MESSAGE_LENGTH_MISMATCH)
      Assert.IsTrue(ArrayHelper.AreEqual(data4, unblindedData), ERROR_MESSAGE_DATA_MISMATCH)
   End Sub

   <TestMethod()> Public Sub TestBlindingLoop()
      Dim blindedData As Byte()
      Dim unblindedData As Byte()

      Dim minimumLength As Integer

      For dataSize As Integer = 1 To 50
         Dim data1 As Byte() = New Byte(dataSize - 1) {}

         SecurePseudoRandomNumberGenerator.GetBytes(data1)

         For i As Short = 1 To 500
            minimumLength = SecurePseudoRandomNumberGenerator.GetInteger(257)

            blindedData = ByteArrayBlinding.BuildBlindedByteArray(data1, minimumLength)
            unblindedData = ByteArrayBlinding.UnBlindByteArray(blindedData)

            Assert.IsTrue(ArrayHelper.AreEqual(data1, unblindedData), ERROR_MESSAGE_DATA_MISMATCH)
         Next
      Next
   End Sub
End Class