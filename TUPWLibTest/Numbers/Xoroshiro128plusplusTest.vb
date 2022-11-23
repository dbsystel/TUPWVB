'
' SPDX-FileCopyrightText: 2022 DB Systel GmbH
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
'    2020-05-06: V1.0.0: Created.
'    2021-07-21: V1.0.1: Corrected instantiation of constants.
'

Option Strict On
Option Explicit On

Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TUPWLib

<TestClass()> Public Class XoRoShiRo128PlusPlusTest
   '
   ' The following numbers are taken from https://commons.apache.org/proper/commons-rng/xref-test/org/apache/commons/rng/core/source64/XoRoShiRo128PlusPlusTest.html .
   '
   Private Shared ReadOnly XS128_SEED As Long() = {
          &H12DE1BABB3C4104L, &HA5A818B8FC5AA503L
      }

   Private Shared ReadOnly X128_EXPECTED_SEQUENCE As Long() = {
          &HF61550E8874B8EAFL, &H125015FCE911E8F6L, &HFF0E6030E39AF1A4L, &HD5738FC2A502673BL,
          &HEF48CDCBEFD84325L, &HB60462C014133DA1L, &HA62C6D8B9F87CD81L, &H52FD609A347198EBL,
          &H3C717475E803BF09L, &H1B6E66B21504A677L, &H528F64243DB486F4L, &H3676015C33FBF0FAL,
          &H3E05F2EA0216A127L, &H373343BB4159FA59L, &HC375C54EBE2F9097L, &H52D85B22744E0574L,
          &H55DD7E34E687524L, &HB749AFC4BC4ED98AL, &H31B972F93D117746L, &HC0E13329779ABC15L,
          &HEE52EC4B4DDC0091L, &HC756C7DD1D6796D6L, &H3CE47F42E211C63EL, &HA635AA7CE5D06101L,
          &HE8054178CBB492C1L, &H3CC3AD122E7DA816L, &HCBAD73CDACAB8FDL, &H20AA1CBC64638B31L,
          &H3BCE572CFE3BC776L, &HCC81E41637090CD8L, &H69CC93E599F51181L, &H2D5C9A4E509F984DL,
          &HF4F3BF08FF627F92L, &H3430E0A0E8670235L, &H75A856B68968F466L, &HDEE1DBBB374913D7L,
          &H9736E33202FBE05BL, &H4BEA0CC1151902A4L, &H9FE7FD9D8DE47D13L, &HF011332584A1C7ABL
      }

   <TestMethod()> Public Sub TestXs128ExpectedSequence()
      Dim xs128 As New XorRoShiRo128PlusPlus(XS128_SEED)

      Dim result As Long() = New Long(0 To X128_EXPECTED_SEQUENCE.Length - 1) {}

      For i As Integer = 0 To X128_EXPECTED_SEQUENCE.Length - 1
         result(i) = xs128.GetLong()
      Next

      Assert.IsTrue(ArrayHelper.AreEqual(X128_EXPECTED_SEQUENCE, result), "XS128pp does not produce expected sequence of numbers")
   End Sub
End Class