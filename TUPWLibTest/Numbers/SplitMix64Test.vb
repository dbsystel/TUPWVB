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
'    2020-05-06: V1.0.0: Created.
'

Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports DB.BCM.TUPW

<TestClass()> Public Class SplitMix64Test
   '
   ' The following numbers are taken from https://commons.apache.org/proper/commons-rng/xref-test/org/apache/commons/rng/core/source64/SplitMix64Test.html .
   '
#Disable Warning CA1707
   Public Const SM64_SEED = &H1A2B3C4D5E6F7531L

   Public Shared ReadOnly SM64_EXPECTED_SEQUENCE As Long() = New Long() {
             &H4141302768C9E9D0L, &H64DF48C4EAB51B1AL, &H4E723B53DBD901B3L, &HEAD8394409DD6454L,
             &H3EF60E485B412A0AL, &HB2A23AEE63AECF38L, &H6CC3B8933C4FA332L, &H9C9E75E031E6FCCBL,
             &HFDDFFB161C9F30FL, &H2D1D75D4E75C12A3L, &HCDCF9D2DDE66DA2EL, &H278BA7D1D142CFECL,
             &H4CA423E66072E606L, &H8F2C3C46EBC70BB7L, &HC9DEF3B1EEAE3E21L, &H8E06670CD3E98BCEL,
             &H2326DEE7DD34747FL, &H3C8FFF64392BB3C1L, &HFC6AA1EBE7916578L, &H3191FB6113694E70L,
             &H3453605F6544DAC6L, &H86CF93E5CDF81801L, &HD764D7E59F724DFL, &HAE1DFB943EBF8659L,
             &H12DE1BABB3C4104L, &HA5A818B8FC5AA503L, &HB124EA2B701F4993L, &H18E0374933D8C782L,
             &H2AF8DF668D68AD55L, &H76E56F59DAA06243L, &HF58C016F0F01E30FL, &H8EEAFA41683DBBF4L,
             &H7BF121347C06677FL, &H4FD0C88D25DB5CCBL, &H99AF3BE9EBE0A272L, &H94F2B33B74D0BDCBL,
             &H24B5D9D7A00A3140L, &H79D983D781A34A3CL, &H582E4A84D595F5ECL, &H7316FE8B0F606D20L
         }
#Enable Warning CA1707

   <TestMethod()> Public Sub TestSm64ExpectedSequence()
      Dim sm64 As New SplitMix64(SM64_SEED)

      Dim result As Long() = New Long(0 To SM64_EXPECTED_SEQUENCE.Length - 1) {}

      For i As Integer = 0 To SM64_EXPECTED_SEQUENCE.Length - 1
         result(i) = sm64.GetLong()
      Next

      Assert.IsTrue(ArrayHelper.AreEqual(SM64_EXPECTED_SEQUENCE, result), "SM64 does not produce expected sequence of numbers")
   End Sub
End Class