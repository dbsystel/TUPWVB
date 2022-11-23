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
' Version: 1.2.0
'
' Change history:
'    2020-05-05: V1.0.0: Created.
'    2020-05-15: V1.0.1: Made constants consistent with types.
'    2020-05-15: V1.1.0: Added exclusive upper boundary methods.
'    2020-05-27: V1.2.0: Refactored function value mapping into own shared class.
'    2020-06-19: V1.2.1: Use new FunctionValueRangeMapperAPI.
'

Option Strict On
Option Explicit On

''' <summary>
''' Common class for a simple pseudo-random number generator that only supports getting numbers one by one.
''' </summary>
''' <remarks>
''' All subclasses need to implement the <see cref="GetLong()"/> method. All other <c>Next</c> methods are implemented here.
''' </remarks>
Public MustInherit Class SimplePseudoRandomNumberGenerator
#Region "Private constants"
   '
   ' It is not possible to write a byte constant in VB.
   '
   Private Const ONE_AS_BYTE As Byte = 1US
   Private Const ONE_AS_SBYTE As SByte = 1S
#End Region

#Region "Public methods"
   '******************************************************************
   ' Public methods
   '******************************************************************

#Region "Simple access methods"
   ''' <summary>
   ''' Get a pseudo-random <see cref="Long"/> value.
   ''' </summary>
   ''' <remarks>
   ''' This method must be overriden.
   ''' </remarks>
   ''' <returns>Never</returns>
   Public MustOverride Function GetLong() As Long

   ''' <summary>
   ''' Get a pseudo-random <see cref="ULong"/> value.
   ''' </summary>
   ''' <returns>Pseudo-random value.</returns>
   Public Function GetUnsignedLong() As ULong
      Return BitwiseIntegerConverter.AsUnsignedLong(GetLong())
   End Function

   ''' <summary>
   ''' Get a pseudo-random <see cref="Integer"/> value.
   ''' </summary>
   ''' <returns>Pseudo-random value.</returns>
   Public Function GetInteger() As Integer
      Return CInt(GetLong() >> 32)
   End Function

   ''' <summary>
   ''' Get a pseudo-random <see cref="UInteger"/> value.
   ''' </summary>
   ''' <returns>Pseudo-random value.</returns>
   Public Function GetUnsignedInteger() As UInteger
      Return BitwiseIntegerConverter.AsUnsignedInteger(GetInteger())
   End Function

   ''' <summary>
   ''' Get a pseudo-random <see cref="Short"/> value.
   ''' </summary>
   ''' <returns>Pseudo-random value.</returns>
   Public Function GetShort() As Short
      Return CShort(GetLong() >> 48)
   End Function

   ''' <summary>
   ''' Get a pseudo-random <see cref="UShort"/> value.
   ''' </summary>
   ''' <returns>Pseudo-random value.</returns>
   Public Function GetUnsignedShort() As UShort
      Return BitwiseIntegerConverter.AsUnsignedShort(GetShort())
   End Function

   ''' <summary>
   ''' Get a pseudo-random <see cref="Byte"/> value.
   ''' </summary>
   ''' <returns>Pseudo-random value.</returns>
   Public Function GetByte() As Byte
      Return BitwiseIntegerConverter.AsUnsignedByte(GetSignedByte())
   End Function

   ''' <summary>
   ''' Get a pseudo-random <see cref="SByte"/> value.
   ''' </summary>
   ''' <returns>Pseudo-random value.</returns>
   Public Function GetSignedByte() As SByte
      Return CSByte(GetLong() >> 56)
   End Function

#End Region

#Region "Inclusive range get methods"
   '
   ' The following methods all implement the same algorithm for getting an equally distributed
   ' pseudo-random number in a range for the specified data type.
   '

   ''' <summary>
   ''' Get a pseudo-random signed byte in a range.
   ''' </summary>
   ''' <param name="fromInclusive">Inclusive start point of the range.</param>
   ''' <param name="toInclusive">Inclusive end point of the range.</param>
   ''' <returns>Pseudo-random signed byte that has a value between <paramref name="fromInclusive"/> and <paramref name="toInclusive"/>.</returns>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="fromInclusive"/> is larger than <paramref name="toInclusive"/>.</exception>
   Public Function GetSignedByte(fromInclusive As SByte, toInclusive As SByte) As SByte
      Return FunctionValueRangeMapper.GetSignedByte(fromInclusive, toInclusive, AddressOf GetByte)
   End Function

   ''' <summary>
   ''' Get a pseudo-random byte in a range.
   ''' </summary>
   ''' <param name="fromInclusive">Inclusive start point of the range.</param>
   ''' <param name="toInclusive">Inclusive end point of the range.</param>
   ''' <returns>Pseudo-random byte that has a value between <paramref name="fromInclusive"/> and <paramref name="toInclusive"/>.</returns>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="fromInclusive"/> is larger than <paramref name="toInclusive"/>.</exception>
   Public Function GetByte(fromInclusive As Byte, toInclusive As Byte) As Byte
      Return FunctionValueRangeMapper.GetByte(fromInclusive, toInclusive, AddressOf GetByte)
   End Function

   ''' <summary>
   ''' Get a pseudo-random unsigned short in a range.
   ''' </summary>
   ''' <param name="fromInclusive">Inclusive start point of the range.</param>
   ''' <param name="toInclusive">Inclusive end point of the range.</param>
   ''' <returns>Pseudo-random unsigned short that has a value between <paramref name="fromInclusive"/> and <paramref name="toInclusive"/>.</returns>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="fromInclusive"/> is larger than <paramref name="toInclusive"/>.</exception>
   Public Function GetUnsignedShort(fromInclusive As UShort, toInclusive As UShort) As UShort
      Return FunctionValueRangeMapper.GetUnsignedShort(fromInclusive, toInclusive, AddressOf GetUnsignedShort)
   End Function

   ''' <summary>
   ''' Get a pseudo-random short in a range.
   ''' </summary>
   ''' <param name="fromInclusive">Inclusive start point of the range.</param>
   ''' <param name="toInclusive">Inclusive end point of the range.</param>
   ''' <returns>Pseudo-random short that has a value between <paramref name="fromInclusive"/> and <paramref name="toInclusive"/>.</returns>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="fromInclusive"/> is larger than <paramref name="toInclusive"/>.</exception>
   Public Function GetShort(fromInclusive As Short, toInclusive As Short) As Short
      Return FunctionValueRangeMapper.GetShort(fromInclusive, toInclusive, AddressOf GetUnsignedShort)
   End Function

   ''' <summary>
   ''' Get a pseudo-random unsigned integer in a range.
   ''' </summary>
   ''' <param name="fromInclusive">Inclusive start point of the range.</param>
   ''' <param name="toInclusive">Inclusive end point of the range.</param>
   ''' <returns>Pseudo-random unsigned integer that has a value between <paramref name="fromInclusive"/> and <paramref name="toInclusive"/>.</returns>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="fromInclusive"/> is larger than <paramref name="toInclusive"/>.</exception>
   Public Function GetUnsignedInteger(fromInclusive As UInteger, toInclusive As UInteger) As UInteger
      Return FunctionValueRangeMapper.GetUnsignedInteger(fromInclusive, toInclusive, AddressOf GetUnsignedInteger)
   End Function

   ''' <summary>
   ''' Get a pseudo-random integer in a range.
   ''' </summary>
   ''' <param name="fromInclusive">Inclusive start point of the range.</param>
   ''' <param name="toInclusive">Inclusive end point of the range.</param>
   ''' <returns>Pseudo-random integer that has a value between <paramref name="fromInclusive"/> and <paramref name="toInclusive"/>.</returns>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="fromInclusive"/> is larger than <paramref name="toInclusive"/>.</exception>
   Public Function GetInteger(fromInclusive As Integer, toInclusive As Integer) As Integer
      Return FunctionValueRangeMapper.GetInteger(fromInclusive, toInclusive, AddressOf GetUnsignedInteger)
   End Function

   ''' <summary>
   ''' Get a pseudo-random unsigned long in a range.
   ''' </summary>
   ''' <param name="fromInclusive">Inclusive start point of the range.</param>
   ''' <param name="toInclusive">Inclusive end point of the range.</param>
   ''' <returns>Pseudo-random unsigned long that has a value between <paramref name="fromInclusive"/> and <paramref name="toInclusive"/>.</returns>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="fromInclusive"/> is larger than <paramref name="toInclusive"/>.</exception>
   Public Function GetUnsignedLong(fromInclusive As ULong, toInclusive As ULong) As ULong
      Return FunctionValueRangeMapper.GetUnsignedLong(fromInclusive, toInclusive, AddressOf GetUnsignedLong)
   End Function

   ''' <summary>
   ''' Get a pseudo-random long in a range.
   ''' </summary>
   ''' <param name="fromInclusive">Inclusive start point of the range.</param>
   ''' <param name="toInclusive">Inclusive end point of the range.</param>
   ''' <returns>Pseudo-random long that has a value between <paramref name="fromInclusive"/> and <paramref name="toInclusive"/>.</returns>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="fromInclusive"/> is larger than <paramref name="toInclusive"/>.</exception>
   Public Function GetLong(fromInclusive As Long, toInclusive As Long) As Long
      Return FunctionValueRangeMapper.GetLong(fromInclusive, toInclusive, AddressOf GetUnsignedLong)
   End Function
#End Region

#Region "Exclusive upper value get methods"
   ''' <summary>
   ''' Get a pseudo-random signed byte in a range from 0 to an upper limit.
   ''' </summary>
   ''' <param name="toExclusive">Exclusive end point of the range.</param>
   ''' <returns>Pseudo-random signed byte that has a value between 0 and <paramref name="toExclusive"/> - 1.</returns>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="toExclusive"/> is less than 0.</exception>
   Public Function GetSignedByte(toExclusive As SByte) As SByte
      Return GetSignedByte(0, toExclusive - ONE_AS_SBYTE)
   End Function

   ''' <summary>
   ''' Get a pseudo-random byte in a range from 0 to an upper limit.
   ''' </summary>
   ''' <param name="toExclusive">Exclusive end point of the range.</param>
   ''' <returns>Pseudo-random byte that has a value between 0 and <paramref name="toExclusive"/> - 1.</returns>
   Public Function GetByte(toExclusive As Byte) As Byte
      Return GetByte(0, toExclusive - ONE_AS_BYTE)
   End Function

   ''' <summary>
   ''' Get a pseudo-random short in a range from 0 to an upper limit.
   ''' </summary>
   ''' <param name="toExclusive">Exclusive end point of the range.</param>
   ''' <returns>Pseudo-random short that has a value between 0 and <paramref name="toExclusive"/> - 1.</returns>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="toExclusive"/> is less than 0.</exception>
   Public Function GetShort(toExclusive As Short) As Short
      Return GetShort(0, toExclusive - 1S)
   End Function

   ''' <summary>
   ''' Get a pseudo-random unsigned short in a range from 0 to an upper limit.
   ''' </summary>
   ''' <param name="toExclusive">Exclusive end point of the range.</param>
   ''' <returns>Pseudo-random unsigned short that has a value between 0 and <paramref name="toExclusive"/> - 1.</returns>
   Public Function GetUnsignedShort(toExclusive As UShort) As UShort
      Return GetUnsignedShort(0, toExclusive - 1US)
   End Function

   ''' <summary>
   ''' Get a pseudo-random integer in a range from 0 to an upper limit.
   ''' </summary>
   ''' <param name="toExclusive">Exclusive end point of the range.</param>
   ''' <returns>Pseudo-random integer that has a value between 0 and <paramref name="toExclusive"/> - 1.</returns>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="toExclusive"/> is less than 0.</exception>
   Public Function GetInteger(toExclusive As Integer) As Integer
      Return GetInteger(0, toExclusive - 1I)
   End Function

   ''' <summary>
   ''' Get a pseudo-random unsigned integer in a range from 0 to an upper limit.
   ''' </summary>
   ''' <param name="toExclusive">Exclusive end point of the range.</param>
   ''' <returns>Pseudo-random unsigned integer that has a value between 0 and <paramref name="toExclusive"/> - 1.</returns>
   Public Function GetUnsignedInteger(toExclusive As UInteger) As UInteger
      Return GetUnsignedInteger(0, toExclusive - 1UI)
   End Function

   ''' <summary>
   ''' Get a pseudo-random long in a range from 0 to an upper limit.
   ''' </summary>
   ''' <param name="toExclusive">Exclusive end point of the range.</param>
   ''' <returns>Pseudo-random long that has a value between 0 and <paramref name="toExclusive"/> - 1.</returns>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="toExclusive"/> is less than 0.</exception>
   Public Function GetLong(toExclusive As Long) As Long
      Return GetLong(0, toExclusive - 1L)
   End Function

   ''' <summary>
   ''' Get a pseudo-random unsigned long in a range from 0 to an upper limit.
   ''' </summary>
   ''' <param name="toExclusive">Exclusive end point of the range.</param>
   ''' <returns>Pseudo-random unsigned long that has a value between 0 and <paramref name="toExclusive"/> - 1.</returns>
   Public Function GetUnsignedLong(toExclusive As ULong) As ULong
      Return GetUnsignedLong(0, toExclusive - 1UL)
   End Function
#End Region

#End Region
End Class
