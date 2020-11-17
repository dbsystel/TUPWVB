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
'    2020-05-12: V1.0.0: Created.
'    2020-05-29: V1.0.1: Test with random data.
'

Imports DB.BCM.TUPW

<TestClass()> Public Class ArbitraryTailPaddingTest
#Region "Private constants"
   Private Const BLOCK_SIZE As Integer = 32
#End Region

#Region "Class variables"
   Private Shared ReadOnly m_Rnd As New Random()
#End Region

#Region "Test methods"
   <TestMethod()> Public Sub TestABytPaddingWorking0DataSize()
      Dim unpaddedSourceData As Byte() = Array.Empty(Of Byte)()

      TestPadAndUnpad(unpaddedSourceData)
   End Sub

   <TestMethod()> Public Sub TestABytPaddingWorkingSmallerThanBlockSize()
      Dim unpaddedSourceData As Byte() = New Byte(0 To BLOCK_SIZE \ 4 - 2) {}

      m_Rnd.NextBytes(unpaddedSourceData)

      TestPadAndUnpad(unpaddedSourceData)
   End Sub

   <TestMethod()> Public Sub TestABytPaddingWorkingEqualBlockSize()
      Dim unpaddedSourceData As Byte() = New Byte(0 To BLOCK_SIZE - 1) {}

      m_Rnd.NextBytes(unpaddedSourceData)

      TestPadAndUnpad(unpaddedSourceData)
   End Sub

   <TestMethod()> Public Sub TestABytPaddingWorkingGreaterThanBlockSize()
      Dim unpaddedSourceData As Byte() = New Byte(0 To BLOCK_SIZE + (BLOCK_SIZE >> 1) - 1) {}

      m_Rnd.NextBytes(unpaddedSourceData)

      TestPadAndUnpad(unpaddedSourceData)
   End Sub
#End Region

#Region "Private methods"
   Private Shared Sub TestPadAndUnpad(unpaddedSourceData As Byte())
      Dim paddedSourceData As Byte() = ArbitraryTailPadding.AddPadding(unpaddedSourceData, BLOCK_SIZE)

      Assert.IsTrue(paddedSourceData.Length > unpaddedSourceData.Length, "Padded data not longer than unpadded data")
      Assert.IsTrue((paddedSourceData.Length Mod BLOCK_SIZE) = 0, "Padding length is not multiple of block size: " & paddedSourceData.Length.ToString())
      Assert.IsTrue((paddedSourceData.Length - unpaddedSourceData.Length) <= BLOCK_SIZE, "Padding is longer than block size")

      Dim unpaddedPaddedSourceData As Byte() = ArbitraryTailPadding.RemovePadding(paddedSourceData)

      Assert.AreEqual(unpaddedSourceData.Length, unpaddedPaddedSourceData.Length, "Lengths are not the same after padding and unpadding")
      Assert.IsTrue(ArrayHelper.AreEqual(unpaddedSourceData, unpaddedPaddedSourceData), "Data ist not the same after padding and unpadding")
   End Sub
#End Region
End Class