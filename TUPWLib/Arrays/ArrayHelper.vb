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
'    2020-04-23: V1.0.0: Created.
'    2020-05-25: V1.0.1: Corrected comments of "CopyOf" methods.
'    2020-05-28: V1.0.2: Check for null added where necessary.
'    2020-05-28: V1.0.3: Corrected comments.
'    2020-10-26: V1.0.4: Added a few comments re. the use of non-short-circuit logic.
'    2021-08-27: V1.1.0: Added SafeClear method.
'
Option Strict On
Option Explicit On

''' <summary>
''' Class with all the array methods missing from the <see cref="Array"/> class.
''' </summary>
Public NotInheritable Class ArrayHelper
#Region "Public methods"
   '
   ' Public methods
   '

#Region "Fast compare methods"
   '=====================
   ' Fast compare methods
   '=====================

   '
   ' The fast compare methods are coded for each basic data type as they are much faster than
   ' the generic method which looks like this:
   '
   '    Public Shared Function FastEquals(Of T As IEquatable)(a1 As T(), a2 As T()) As Boolean
   '       Dim result As Boolean = False
   '
   '       If a1.Length = a2.Length Then
   '          result = True
   '
   '          For i As Integer = 0 To a1.Length - 1
   '             If Not a1(i).Equals(a2(i)) Then
   '                result = False
   '                Exit For
   '             End If
   '          Next
   '       End If
   '
   '       Return result
   '   End Function
   '
   ' Here the much slower 'Equals' method is called instead of just comparing two values which is much faster.
   '

   ''' <summary>
   ''' Compare two signed byte arrays as this is missing from the <see cref="Array"/> class.
   ''' </summary>
   '''
   ''' <remarks>
   ''' This is a fast compare that is not suitable for cryptographic operations.
   ''' This method should have been a part of the <see cref="Array"/> class.
   ''' </remarks>
   '''
   ''' <param name="a1">First signed byte array to compare.</param>
   ''' <param name="a2">Second signed byte array to compare.</param>
   ''' <exception cref="ArgumentNullException">Thrown if any array is <c>Nothing</c>.</exception>
   ''' <returns><c>true</c>, if both arrays are equal, <c>false</c>, if not.</returns>
   Public Shared Function AreEqual(a1 As SByte(), a2 As SByte()) As Boolean
      CheckArraysForNull(a1, a2)

      Dim result As Boolean = False

      If a1.Length = a2.Length Then
         result = True

         For i As Integer = 0 To a1.Length - 1
            If a1(i) <> a2(i) Then
               result = False
               Exit For
            End If
         Next
      End If

      Return result
   End Function

   ''' <summary>
   ''' Compare two byte arrays as this is missing from the <see cref="Array"/> class.
   ''' </summary>
   '''
   ''' <remarks>
   ''' This is a fast compare that is not suitable for cryptographic operations.
   ''' This method should have been a part of the <see cref="Array"/> class.
   ''' </remarks>
   '''
   ''' <param name="a1">First byte array to compare.</param>
   ''' <param name="a2">Second byte array to compare.</param>
   ''' <exception cref="ArgumentNullException">Thrown if any array is <c>Nothing</c>.</exception>
   ''' <returns><c>true</c>, if both arrays are equal, <c>false</c>, if not.</returns>
   Public Shared Function AreEqual(a1 As Byte(), a2 As Byte()) As Boolean
      CheckArraysForNull(a1, a2)

      Dim result As Boolean = False

      If a1.Length = a2.Length Then
         result = True

         For i As Integer = 0 To a1.Length - 1
            If a1(i) <> a2(i) Then
               result = False
               Exit For
            End If
         Next
      End If

      Return result
   End Function

   ''' <summary>
   ''' Compare two short arrays as this is missing from the <see cref="Array"/> class.
   ''' </summary>
   '''
   ''' <remarks>
   ''' This is a fast compare that is not suitable for cryptographic operations.
   ''' This method should have been a part of the <see cref="Array"/> class.
   ''' </remarks>
   '''
   ''' <param name="a1">First short array to compare.</param>
   ''' <param name="a2">Second short array to compare.</param>
   ''' <exception cref="ArgumentNullException">Thrown if any array is <c>Nothing</c>.</exception>
   ''' <returns><c>true</c>, if both arrays are equal, <c>false</c>, if not.</returns>
   Public Shared Function AreEqual(a1 As Short(), a2 As Short()) As Boolean
      CheckArraysForNull(a1, a2)

      Dim result As Boolean = False

      If a1.Length = a2.Length Then
         result = True

         For i As Integer = 0 To a1.Length - 1
            If a1(i) <> a2(i) Then
               result = False
               Exit For
            End If
         Next
      End If

      Return result
   End Function

   ''' <summary>
   ''' Compare two unsigned short arrays as this is missing from the <see cref="Array"/> class.
   ''' </summary>
   '''
   ''' <remarks>
   ''' This is a fast compare that is not suitable for cryptographic operations.
   ''' This method should have been a part of the <see cref="Array"/> class.
   ''' </remarks>
   '''
   ''' <param name="a1">First unsigned short array to compare.</param>
   ''' <param name="a2">Second unsigned short array to compare.</param>
   ''' <exception cref="ArgumentNullException">Thrown if any array is <c>Nothing</c>.</exception>
   ''' <returns><c>true</c>, if both arrays are equal, <c>false</c>, if not.</returns>
   Public Shared Function AreEqual(a1 As UShort(), a2 As UShort()) As Boolean
      CheckArraysForNull(a1, a2)

      Dim result As Boolean = False

      If a1.Length = a2.Length Then
         result = True

         For i As Integer = 0 To a1.Length - 1
            If a1(i) <> a2(i) Then
               result = False
               Exit For
            End If
         Next
      End If

      Return result
   End Function

   ''' <summary>
   ''' Compare two integer arrays as this is missing from the <see cref="Array"/> class.
   ''' </summary>
   '''
   ''' <remarks>
   ''' This is a fast compare that is not suitable for cryptographic operations.
   ''' This method should have been a part of the <see cref="Array"/> class.
   ''' </remarks>
   '''
   ''' <param name="a1">First integer array to compare.</param>
   ''' <param name="a2">Second integer array to compare.</param>
   ''' <exception cref="ArgumentNullException">Thrown if any array is <c>Nothing</c>.</exception>
   ''' <returns><c>true</c>, if both arrays are equal, <c>false</c>, if not.</returns>
   Public Shared Function AreEqual(a1 As Integer(), a2 As Integer()) As Boolean
      CheckArraysForNull(a1, a2)

      Dim result As Boolean = False

      If a1.Length = a2.Length Then
         result = True

         For i As Integer = 0 To a1.Length - 1
            If a1(i) <> a2(i) Then
               result = False
               Exit For
            End If
         Next
      End If

      Return result
   End Function

   ''' <summary>
   ''' Compare two unsigned integer arrays as this is missing from the <see cref="Array"/> class.
   ''' </summary>
   '''
   ''' <remarks>
   ''' This is a fast compare that is not suitable for cryptographic operations.
   ''' This method should have been a part of the <see cref="Array"/> class.
   ''' </remarks>
   '''
   ''' <param name="a1">First unsigned integer array to compare.</param>
   ''' <param name="a2">Second unsigned integer array to compare.</param>
   ''' <exception cref="ArgumentNullException">Thrown if any array is <c>Nothing</c>.</exception>
   ''' <returns><c>true</c>, if both arrays are equal, <c>false</c>, if not.</returns>
   Public Shared Function AreEqual(a1 As UInteger(), a2 As UInteger()) As Boolean
      CheckArraysForNull(a1, a2)

      Dim result As Boolean = False

      If a1.Length = a2.Length Then
         result = True

         For i As Integer = 0 To a1.Length - 1
            If a1(i) <> a2(i) Then
               result = False
               Exit For
            End If
         Next
      End If

      Return result
   End Function

   ''' <summary>
   ''' Compare two long arrays as this is missing from the <see cref="Array"/> class.
   ''' </summary>
   '''
   ''' <remarks>
   ''' This is a fast compare that is not suitable for cryptographic operations.
   ''' This method should have been a part of the <see cref="Array"/> class.
   ''' </remarks>
   '''
   ''' <param name="a1">First long array to compare.</param>
   ''' <param name="a2">Second long array to compare.</param>
   ''' <exception cref="ArgumentNullException">Thrown if any array is <c>Nothing</c>.</exception>
   ''' <returns><c>true</c>, if both arrays are equal, <c>false</c>, if not.</returns>
   Public Shared Function AreEqual(a1 As Long(), a2 As Long()) As Boolean
      CheckArraysForNull(a1, a2)

      Dim result As Boolean = False

      If a1.Length = a2.Length Then
         result = True

         For i As Integer = 0 To a1.Length - 1
            If a1(i) <> a2(i) Then
               result = False
               Exit For
            End If
         Next
      End If

      Return result
   End Function

   ''' <summary>
   ''' Compare two unsigned long arrays as this is missing from the <see cref="Array"/> class.
   ''' </summary>
   '''
   ''' <remarks>
   ''' This is a fast compare that is not suitable for cryptographic operations.
   ''' This method should have been a part of the <see cref="Array"/> class.
   ''' </remarks>
   '''
   ''' <param name="a1">First unsigned long array to compare.</param>
   ''' <param name="a2">Second unsigned long array to compare.</param>
   ''' <exception cref="ArgumentNullException">Thrown if any array is <c>Nothing</c>.</exception>
   ''' <returns><c>true</c>, if both arrays are equal, <c>false</c>, if not.</returns>
   Public Shared Function AreEqual(a1 As ULong(), a2 As ULong()) As Boolean
      CheckArraysForNull(a1, a2)

      Dim result As Boolean = False

      If a1.Length = a2.Length Then
         result = True

         For i As Integer = 0 To a1.Length - 1
            If a1(i) <> a2(i) Then
               result = False
               Exit For
            End If
         Next
      End If

      Return result
   End Function

   ''' <summary>
   ''' Compare two character arrays as this is missing from the <see cref="Array"/> class.
   ''' </summary>
   '''
   ''' <remarks>
   ''' This is a fast compare that is not suitable for cryptographic operations.
   ''' This method should have been a part of the <see cref="Array"/> class.
   ''' </remarks>
   '''
   ''' <param name="a1">First character array to compare.</param>
   ''' <param name="a2">Second character array to compare.</param>
   ''' <exception cref="ArgumentNullException">Thrown if any array is <c>Nothing</c>.</exception>
   ''' <returns><c>true</c>, if both arrays are equal, <c>false</c>, if not.</returns>
   Public Shared Function AreEqual(a1 As Char(), a2 As Char()) As Boolean
      CheckArraysForNull(a1, a2)

      Dim result As Boolean = False

      If a1.Length = a2.Length Then
         result = True

         For i As Integer = 0 To a1.Length - 1
            If a1(i) <> a2(i) Then
               result = False
               Exit For
            End If
         Next
      End If

      Return result
   End Function
#End Region

#Region "Secure compare methods"
   '=======================
   ' Secure compare methods
   '=======================

   '
   ' The secure compare methods are coded for each basic data type as they are much faster than
   ' the generic method which looks like this:
   '
   '    Public Shared Function SecureAreEqual(Of T As IEquatable)(a1 As T(), a2 As T()) As Boolean
   '       Dim sumEquals As Boolean = True
   '
   '       Dim compareLength As Integer = a1.Length
   '
   '       If a2.Length < compareLength Then _
   '          compareLength = a2.Length
   '
   '       For i As Integer = 0 To compareLength - 1
   '          sumEquals = sumEquals And a1(i).Equals(a2(i))
   '       Next
   '
   '       Return (a1.Length = a2.Length) And sumEquals
   '    End Function
   '
   ' Here the much slower 'Equals' method is called instead of just Xor-ing two values which is much faster.
   '


   ''' <summary>
   ''' Constant time signed byte array compare.
   ''' </summary>
   '''
   ''' <remarks>
   ''' This method takes a constant time to compare two signed byte arrays, i.e. it will
   ''' take the same time to run if the arrays are equal and if they are not
   ''' equal. This makes it impossible to attack a cryptographic operation by
   ''' measuring the time it takes to complete a compare operation.
   ''' </remarks>
   '''
   ''' <param name="a1">First signed byte array to compare.</param>
   ''' <param name="a2">Second signed byte array to compare.</param>
   ''' <exception cref="ArgumentNullException">Thrown if any array is <c>Nothing</c>.</exception>
   ''' <returns><c>true</c>, if both arrays are equal, <c>false</c>, if not.</returns>
   Public Shared Function SecureAreEqual(a1 As SByte(), a2 As SByte()) As Boolean
      CheckArraysForNull(a1, a2)

      Dim compareLength As Integer = a1.Length

      If a2.Length < compareLength Then _
         compareLength = a2.Length

      Const ZERO_SUM As SByte = 0S

      Dim xorSum As SByte = ZERO_SUM

      For i As Integer = 0 To compareLength - 1
         xorSum = xorSum Or (a1(i) Xor a2(i))
      Next

      '
      ' Do *not* change 'And' to 'AndAlso' here!
      ' For a secure comparison this statement *must* use non-short-circuit logic!
      '
#Disable Warning S2178 ' Short-circuit logic should be used in boolean contexts
      Return (a1.Length = a2.Length) And (xorSum = ZERO_SUM)
#Enable Warning S2178 ' Short-circuit logic should be used in boolean contexts
   End Function

   ''' <summary>
   ''' Constant time byte array compare.
   ''' </summary>
   '''
   ''' <remarks>
   ''' This method takes a constant time to compare two byte arrays, i.e. it will
   ''' take the same time to run if the arrays are equal and if they are not
   ''' equal. This makes it impossible to attack a cryptographic operation by
   ''' measuring the time it takes to complete a compare operation.
   ''' </remarks>
   '''
   ''' <param name="a1">First byte array to compare.</param>
   ''' <param name="a2">Second byte array to compare.</param>
   ''' <exception cref="ArgumentNullException">Thrown if any array is <c>Nothing</c>.</exception>
   ''' <returns><c>true</c>, if both arrays are equal, <c>false</c>, if not.</returns>
   Public Shared Function SecureAreEqual(a1 As Byte(), a2 As Byte()) As Boolean
      CheckArraysForNull(a1, a2)

      Dim compareLength As Integer = a1.Length

      If a2.Length < compareLength Then _
         compareLength = a2.Length

      Const ZERO_SUM As Byte = 0US

      Dim xorSum As Byte = ZERO_SUM

      For i As Integer = 0 To compareLength - 1
         xorSum = xorSum Or (a1(i) Xor a2(i))
      Next

      '
      ' Do *not* change 'And' to 'AndAlso' here!
      ' For a secure comparison this statement *must* use non-short-circuit logic!
      '
#Disable Warning S2178 ' Short-circuit logic should be used in boolean contexts
      Return (a1.Length = a2.Length) And (xorSum = ZERO_SUM)
#Enable Warning S2178 ' Short-circuit logic should be used in boolean contexts
   End Function

   ''' <summary>
   ''' Constant time short array compare.
   ''' </summary>
   '''
   ''' <remarks>
   ''' This method takes a constant time to compare two short arrays, i.e. it will
   ''' take the same time to run if the arrays are equal and if they are not
   ''' equal. This makes it impossible to attack a cryptographic operation by
   ''' measuring the time it takes to complete a compare operation.
   ''' </remarks>
   '''
   ''' <param name="a1">First short array to compare.</param>
   ''' <param name="a2">Second short array to compare.</param>
   ''' <exception cref="ArgumentNullException">Thrown if any array is <c>Nothing</c>.</exception>
   ''' <returns><c>true</c>, if both arrays are equal, <c>false</c>, if not.</returns>
   Public Shared Function SecureAreEqual(a1 As Short(), a2 As Short()) As Boolean
      CheckArraysForNull(a1, a2)

      Dim compareLength As Integer = a1.Length

      Const ZERO_SUM As Short = 0S

      Dim xorSum As Short = ZERO_SUM

      If a2.Length < compareLength Then _
         compareLength = a2.Length

      For i As Integer = 0 To compareLength - 1
         xorSum = xorSum Or (a1(i) Xor a2(i))
      Next

      '
      ' Do *not* change 'And' to 'AndAlso' here!
      ' For a secure comparison this statement *must* use non-short-circuit logic!
      '
#Disable Warning S2178 ' Short-circuit logic should be used in boolean contexts
      Return (a1.Length = a2.Length) And (xorSum = ZERO_SUM)
#Enable Warning S2178 ' Short-circuit logic should be used in boolean contexts
   End Function

   ''' <summary>
   ''' Constant time unsigned short array compare.
   ''' </summary>
   '''
   ''' <remarks>
   ''' This method takes a constant time to compare two unsigned short arrays, i.e. it will
   ''' take the same time to run if the arrays are equal and if they are not
   ''' equal. This makes it impossible to attack a cryptographic operation by
   ''' measuring the time it takes to complete a compare operation.
   ''' </remarks>
   '''
   ''' <param name="a1">First unsigned short array to compare.</param>
   ''' <param name="a2">Second unsigned short array to compare.</param>
   ''' <exception cref="ArgumentNullException">Thrown if any array is <c>Nothing</c>.</exception>
   ''' <returns><c>true</c>, if both arrays are equal, <c>false</c>, if not.</returns>
   Public Shared Function SecureAreEqual(a1 As UShort(), a2 As UShort()) As Boolean
      CheckArraysForNull(a1, a2)

      Dim compareLength As Integer = a1.Length

      If a2.Length < compareLength Then _
         compareLength = a2.Length

      Const ZERO_SUM As UShort = 0US

      Dim xorSum As UShort = ZERO_SUM

      For i As Integer = 0 To compareLength - 1
         xorSum = xorSum Or (a1(i) Xor a2(i))
      Next

      '
      ' Do *not* change 'And' to 'AndAlso' here!
      ' For a secure comparison this statement *must* use non-short-circuit logic!
      '
#Disable Warning S2178 ' Short-circuit logic should be used in boolean contexts
      Return (a1.Length = a2.Length) And (xorSum = ZERO_SUM)
#Enable Warning S2178 ' Short-circuit logic should be used in boolean contexts
   End Function

   ''' <summary>
   ''' Constant time integer array compare.
   ''' </summary>
   '''
   ''' <remarks>
   ''' This method takes a constant time to compare two integer arrays, i.e. it will
   ''' take the same time to run if the arrays are equal and if they are not
   ''' equal. This makes it impossible to attack a cryptographic operation by
   ''' measuring the time it takes to complete a compare operation.
   ''' </remarks>
   '''
   ''' <param name="a1">First integer array to compare.</param>
   ''' <param name="a2">Second integer array to compare.</param>
   ''' <exception cref="ArgumentNullException">Thrown if any array is <c>Nothing</c>.</exception>
   ''' <returns><c>true</c>, if both arrays are equal, <c>false</c>, if not.</returns>
   Public Shared Function SecureAreEqual(a1 As Integer(), a2 As Integer()) As Boolean
      CheckArraysForNull(a1, a2)

      Dim compareLength As Integer = a1.Length

      If a2.Length < compareLength Then _
         compareLength = a2.Length

      Const ZERO_SUM As Integer = 0I

      Dim xorSum As Integer = ZERO_SUM

      For i As Integer = 0 To compareLength - 1
         xorSum = xorSum Or (a1(i) Xor a2(i))
      Next

      '
      ' Do *not* change 'And' to 'AndAlso' here!
      ' For a secure comparison this statement *must* use non-short-circuit logic!
      '
#Disable Warning S2178 ' Short-circuit logic should be used in boolean contexts
      Return (a1.Length = a2.Length) And (xorSum = ZERO_SUM)
#Enable Warning S2178 ' Short-circuit logic should be used in boolean contexts
   End Function

   ''' <summary>
   ''' Constant time unsigned integer array compare.
   ''' </summary>
   '''
   ''' <remarks>
   ''' This method takes a constant time to compare two unsigned integer arrays, i.e. it will
   ''' take the same time to run if the arrays are equal and if they are not
   ''' equal. This makes it impossible to attack a cryptographic operation by
   ''' measuring the time it takes to complete a compare operation.
   ''' </remarks>
   '''
   ''' <param name="a1">First unsigned integer array to compare.</param>
   ''' <param name="a2">Second unsigned integer array to compare.</param>
   ''' <exception cref="ArgumentNullException">Thrown if any array is <c>Nothing</c>.</exception>
   ''' <returns><c>true</c>, if both arrays are equal, <c>false</c>, if not.</returns>
   Public Shared Function SecureAreEqual(a1 As UInteger(), a2 As UInteger()) As Boolean
      CheckArraysForNull(a1, a2)

      Dim compareLength As Integer = a1.Length

      Const ZERO_SUM As UInteger = 0UI

      Dim xorSum As UInteger = ZERO_SUM

      If a2.Length < compareLength Then _
         compareLength = a2.Length

      For i As Integer = 0 To compareLength - 1
         xorSum = xorSum Or (a1(i) Xor a2(i))
      Next

      '
      ' Do *not* change 'And' to 'AndAlso' here!
      ' For a secure comparison this statement *must* use non-short-circuit logic!
      '
#Disable Warning S2178 ' Short-circuit logic should be used in boolean contexts
      Return (a1.Length = a2.Length) And (xorSum = ZERO_SUM)
#Enable Warning S2178 ' Short-circuit logic should be used in boolean contexts
   End Function

   ''' <summary>
   ''' Constant time long array compare.
   ''' </summary>
   '''
   ''' <remarks>
   ''' This method takes a constant time to compare two long arrays, i.e. it will
   ''' take the same time to run if the arrays are equal and if they are not
   ''' equal. This makes it impossible to attack a cryptographic operation by
   ''' measuring the time it takes to complete a compare operation.
   ''' </remarks>
   '''
   ''' <param name="a1">First long array to compare.</param>
   ''' <param name="a2">Second long array to compare.</param>
   ''' <exception cref="ArgumentNullException">Thrown if any array is <c>Nothing</c>.</exception>
   ''' <returns><c>true</c>, if both arrays are equal, <c>false</c>, if not.</returns>
   Public Shared Function SecureAreEqual(a1 As Long(), a2 As Long()) As Boolean
      CheckArraysForNull(a1, a2)

      Dim compareLength As Integer = a1.Length

      If a2.Length < compareLength Then _
         compareLength = a2.Length

      Const ZERO_SUM As Long = 0L

      Dim xorSum As Long = ZERO_SUM

      For i As Integer = 0 To compareLength - 1
         xorSum = xorSum Or (a1(i) Xor a2(i))
      Next

      '
      ' Do *not* change 'And' to 'AndAlso' here!
      ' For a secure comparison this statement *must* use non-short-circuit logic!
      '
#Disable Warning S2178 ' Short-circuit logic should be used in boolean contexts
      Return (a1.Length = a2.Length) And (xorSum = ZERO_SUM)
#Enable Warning S2178 ' Short-circuit logic should be used in boolean contexts
   End Function

   ''' <summary>
   ''' Constant time unsigned long array compare.
   ''' </summary>
   '''
   ''' <remarks>
   ''' This method takes a constant time to compare two unsigned long arrays, i.e. it will
   ''' take the same time to run if the arrays are equal and if they are not
   ''' equal. This makes it impossible to attack a cryptographic operation by
   ''' measuring the time it takes to complete a compare operation.
   ''' </remarks>
   '''
   ''' <param name="a1">First unsigned long array to compare.</param>
   ''' <param name="a2">Second unsigned long array to compare.</param>
   ''' <exception cref="ArgumentNullException">Thrown if any array is <c>Nothing</c>.</exception>
   ''' <returns><c>true</c>, if both arrays are equal, <c>false</c>, if not.</returns>
   Public Shared Function SecureAreEqual(a1 As ULong(), a2 As ULong()) As Boolean
      CheckArraysForNull(a1, a2)

      Dim compareLength As Integer = a1.Length

      If a2.Length < compareLength Then _
         compareLength = a2.Length

      Const ZERO_SUM As ULong = 0UL

      Dim xorSum As ULong = ZERO_SUM

      For i As Integer = 0 To compareLength - 1
         xorSum = xorSum Or (a1(i) Xor a2(i))
      Next

      '
      ' Do *not* change 'And' to 'AndAlso' here!
      ' For a secure comparison this statement *must* use non-short-circuit logic!
      '
#Disable Warning S2178 ' Short-circuit logic should be used in boolean contexts
      Return (a1.Length = a2.Length) And (xorSum = ZERO_SUM)
#Enable Warning S2178 ' Short-circuit logic should be used in boolean contexts
   End Function

   ''' <summary>
   ''' Constant time character array compare.
   ''' </summary>
   '''
   ''' <remarks>
   ''' This method takes a constant time to compare two character arrays, i.e. it will
   ''' take the same time to run if the arrays are equal and if they are not
   ''' equal. This makes it impossible to attack a cryptographic operation by
   ''' measuring the time it takes to complete a compare operation.
   ''' </remarks>
   '''
   ''' <param name="a1">First character array to compare.</param>
   ''' <param name="a2">Second character array to compare.</param>
   ''' <exception cref="ArgumentNullException">Thrown if any array is <c>Nothing</c>.</exception>
   ''' <returns><c>true</c>, if both arrays are equal, <c>false</c>, if not.</returns>
   Public Shared Function SecureAreEqual(a1 As Char(), a2 As Char()) As Boolean
      CheckArraysForNull(a1, a2)

      Dim compareLength As Integer = a1.Length

      If a2.Length < compareLength Then _
         compareLength = a2.Length

      Dim compareResult As Boolean = (a1.Length = a2.Length)

      For i As Integer = 0 To compareLength - 1
         '
         ' Do *not* change 'And' to 'AndAlso' here!
         ' For a secure comparison this statement *must* use non-short-circuit logic!
         '
#Disable Warning S2178 ' Short-circuit logic should be used in boolean contexts
         compareResult = compareResult And (a1(i) = a2(i))
#Enable Warning S2178 ' Short-circuit logic should be used in boolean contexts
      Next

      Return compareResult
   End Function
#End Region

#Region "Fill methods"
   '=============
   ' Fill methods
   '=============

   ''' <summary>
   ''' Fill an array with a specified value starting from offset with a specified length.
   ''' </summary>
   ''' <typeparam name="T">Type of elements of the array.</typeparam>
   ''' <param name="destinationArray">The array to fill.</param>
   ''' <param name="aValue">The value used to fill the array.</param>
   ''' <param name="offset">The start index in the array where the filling starts.</param>
   ''' <param name="count">Number of array elements to fill.</param>
   ''' <exception cref="ArgumentNullException">Thrown when <paramref name="destinationArray"/> is <c>Nothing</c>.</exception>
   ''' <exception cref="ArgumentException">Thrown when <paramref name="offset"/> is less than 0 or 
   ''' <paramref name="offset"/> is beyond the upper bound of <paramref name="destinationArray"/> or
   ''' <paramref name="count"/> is less than 0 or <paramref name="destinationArray"/> is not large enough to be filled with that
   ''' many values.</exception>
   Public Shared Sub Fill(Of T)(destinationArray As T(), aValue As T, offset As Integer, count As Integer)
      CheckArrayOffsetCountMustFit(NameOf(destinationArray), destinationArray, offset, count)

      For i = offset To offset + count - 1
         destinationArray(i) = aValue
      Next
   End Sub

   ''' <summary>
   ''' Fill an array with a specified value starting from an index until the end of the array.
   ''' </summary>
   ''' <typeparam name="T">Type of elements of the array.</typeparam>
   ''' <param name="destinationArray">The array to fill.</param>
   ''' <param name="aValue">The value used to fill the array.</param>
   ''' <param name="offset">The start index in the array where the filling starts.</param>
   ''' <exception cref="ArgumentNullException">Thrown when <paramref name="destinationArray"/> is <c>Nothing</c>.</exception>
   ''' <exception cref="ArgumentException">Thrown when <paramref name="offset"/> is less than 0 or 
   ''' <paramref name="offset"/> is beyond the upper bound of <paramref name="destinationArray"/>.</exception>
   Public Shared Sub Fill(Of T)(destinationArray As T(), aValue As T, offset As Integer)
      If destinationArray Is Nothing Then _
         Throw New ArgumentNullException(NameOf(destinationArray))

      Fill(destinationArray, aValue, offset, destinationArray.Length - offset)
   End Sub

   ''' <summary>
   ''' Fill an array with a specified value starting.
   ''' </summary>
   ''' <typeparam name="T">Type of elements of the array.</typeparam>
   ''' <param name="destinationArray">The array to fill.</param>
   ''' <param name="aValue">The value used to fill the array.</param>
   ''' <exception cref="ArgumentNullException">Thrown when <paramref name="destinationArray"/> is <c>Nothing</c>.</exception>
   Public Shared Sub Fill(Of T)(destinationArray As T(), aValue As T)
      Fill(destinationArray, aValue, 0)
   End Sub
#End Region

#Region "Clear methods"
   '==============
   ' Clear methods
   '==============

   ''' <summary>
   ''' Clear an array without the need to provide all those unnecessary parameters.
   ''' </summary>
   ''' <remarks>
   ''' This method ought to be part of the <see cref="Array"/> class.
   ''' </remarks>
   ''' <param name="arrayToClear">The array to clear.</param>
   ''' <exception cref="ArgumentNullException">Throw if <paramref name="arrayToClear"/> is <c>Nothing</c>.</exception>
   Public Shared Sub Clear(arrayToClear As Array)
      If arrayToClear Is Nothing Then _
         Throw New ArgumentNullException(NameOf(arrayToClear))

      Array.Clear(arrayToClear, 0, arrayToClear.Length)
   End Sub

   ''' <summary>
   ''' Clear an array without the need to provide all those unnecessary parameters if it is not nothing.
   ''' </summary>
   ''' <remarks>
   ''' This method ought to be part of the <see cref="Array"/> class.
   ''' </remarks>
   ''' <param name="arrayToClear">The array to clear.</param>
   Public Shared Sub SafeClear(arrayToClear As Array)
      If arrayToClear IsNot Nothing Then _
         Array.Clear(arrayToClear, 0, arrayToClear.Length)
   End Sub
#End Region

#Region "CopyOf methods"
   ' ==============
   ' CopyOf methods
   ' ==============

   ''' <summary>
   ''' Create a copy of an array with a specified length from a specified offset.
   ''' </summary>
   ''' <typeparam name="T">Type of elements of the array.</typeparam>
   ''' <param name="sourceArray">>Source array to copy.</param>
   ''' <param name="offset">Offset where to start the copy.</param>
   ''' <param name="count">Length of the new array (may be longer or shorter than the original array).</param>
   ''' <returns>Copy of <paramref name="sourceArray"/> from <paramref name="offset"/> for <paramref name="count"/> elements.</returns>
   ''' <exception cref="ArgumentNullException">Thrown if <paramref name="sourceArray"/> is <c>Nothing</c>.</exception>
   ''' <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="offset"/> or <paramref name="count"/> are
   ''' less than 0 or <paramref name="offset"/> is larger than the length of <paramref name="sourceArray"/>.</exception>
   Public Shared Function CopyOf(Of T)(sourceArray As T(), offset As Integer, count As Integer) As T()
      CheckArrayOffsetCount(NameOf(sourceArray), sourceArray, offset, count)

      Dim result As T() = New T(0 To count - 1) {}

      Dim copyLength As Integer = sourceArray.Length - offset

      If count < copyLength Then _
         copyLength = count

      Array.Copy(sourceArray, offset, result, 0, copyLength)

      Return result
   End Function

   ''' <summary>
   ''' Create a copy of an array with a specified length.
   ''' </summary>
   ''' <remarks>
   ''' The length may be larger or smaller than the length of the source array.
   ''' </remarks>
   ''' <typeparam name="T">Type of elements of the array.</typeparam>
   ''' <param name="sourceArray">Source array to copy.</param>
   ''' <param name="count">Length of the new array (may be longer or shorter than the original array).</param>
   ''' <returns>Copy of <paramref name="sourceArray"/> with length <paramref name="count"/>.</returns>
   ''' <exception cref="ArgumentNullException">Thrown if <paramref name="sourceArray"/> is <c>Nothing</c>.</exception>
   ''' <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="count"/> is less than 0.</exception>
   Public Shared Function CopyOf(Of T)(sourceArray As T(), count As Integer) As T()
      Return CopyOf(sourceArray, 0, count)
   End Function

   ''' <summary>
   ''' Create a copy of an array.
   ''' </summary>
   ''' <typeparam name="T">Type of elements of the array.</typeparam>
   ''' <param name="sourceArray">Source array to copy.</param>
   ''' <returns>Copy of <paramref name="sourceArray"/>.</returns>
   ''' <exception cref="ArgumentNullException">Thrown if <paramref name="sourceArray"/> is <c>Nothing</c>.</exception>
   Public Shared Function CopyOf(Of T)(sourceArray As T()) As T()
      If sourceArray Is Nothing Then _
         Throw New ArgumentNullException(NameOf(sourceArray))

      Return CopyOf(sourceArray, 0, sourceArray.Length)
   End Function
#End Region
#End Region

#Region "Private methods"
   '
   ' Private methods
   '
#Region "Check methods"
   ''' <summary>
   ''' Check arrays for null.
   ''' </summary>
   ''' <param name="a1">First array to check.</param>
   ''' <param name="a2">Seconmd array to check.</param>
   ''' <exception cref="ArgumentNullException">Thrown if either array is <c>Nothing</c>.</exception>
   Private Shared Sub CheckArraysForNull(a1 As Array, a2 As Array)
      If a1 Is Nothing Then _
         Throw New ArgumentNullException(NameOf(a1))

      If a2 Is Nothing Then _
         Throw New ArgumentNullException(NameOf(a1))
   End Sub

   ''' <summary>
   ''' Check array, offset and count parameters
   ''' </summary>
   ''' <param name="arrayName">Name of the array to check.</param>
   ''' <param name="anArray">Array to check.</param>
   ''' <param name="offset">Start index in the array.</param>
   ''' <param name="count">Number of array elements.</param>
   ''' <exception cref="ArgumentNullException">Thrown if <paramref name="anArray"/> is <c>Nothing</c>.</exception>
   ''' <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="offset"/> or <paramref name="count"/> are
   ''' less than 0 or <paramref name="offset"/> is larger than the length of <paramref name="anArray"/>.</exception>
   Private Shared Sub CheckArrayOffsetCount(arrayName As String, anArray As Array, offset As Integer, count As Integer)
      If anArray Is Nothing Then _
         Throw New ArgumentNullException(arrayName)

      If offset < 0 Then _
         Throw New ArgumentOutOfRangeException(NameOf(offset))

      '
      ' A source array of length 0 has a lower bound of 0 and an upper bound of -1. So we must not check the offset
      ' if the source array has a length of 0.
      '
      If anArray.Length > 0 AndAlso offset >= anArray.Length Then _
         Throw New ArgumentOutOfRangeException(NameOf(offset))

      If count < 0 Then _
         Throw New ArgumentOutOfRangeException(NameOf(count))
   End Sub

   ''' <summary>
   ''' Check array, offset and count parameters
   ''' </summary>
   ''' <param name="arrayName">Name of the array to check.</param>
   ''' <param name="anArray">Array to check.</param>
   ''' <param name="offset">Start index in the array.</param>
   ''' <param name="count">Number of array elements.</param>
   ''' <exception cref="ArgumentNullException">Thrown if <paramref name="anArray"/> is <c>Nothing</c>.</exception>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="offset"/> is less than 0 or greater than
   ''' or equal to the length of <paramref name="anArray"/> or <paramref name="count"/> is less than 0, or
   ''' <paramref name="offset"/> + <paramref name="count"/> exceed the length of the array.</exception>
   Private Shared Sub CheckArrayOffsetCountMustFit(arrayName As String, anArray As Array, offset As Integer, count As Integer)
      CheckArrayOffsetCount(arrayName, anArray, offset, count)

      If (offset + count) > anArray.Length Then _
         Throw New ArgumentException("Array is not large enough for count elements from offset")
   End Sub
#End Region
#End Region
End Class
