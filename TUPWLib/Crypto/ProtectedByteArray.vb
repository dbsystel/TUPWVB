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
'    2020-04-23: V1.0.0: Created.
'

''' <summary>
''' Stores a byte array in a protected form where "protection" means the following:
''' 1. The data are only stored in an obfuscated form 
''' 2. the data are cleared from memory when "Dispose" is called.
''' </summary>
''' <remarks>
''' The content of the Byte array can not be changed after it has been set
''' with the constructor.
''' </remarks>
Public Class ProtectedByteArray : Implements IDisposable

#Region "Instance variables"
   '******************************************************************
   ' Instance variables
   '******************************************************************

   Private m_ProtectedArray As ShuffledByteArray
   Private m_Obfuscation As ShuffledByteArray
#End Region

#Region "Constructors"
   '******************************************************************
   ' Constructors
   '******************************************************************

   ''' <summary>
   ''' Creates a new <see cref="ProtectedByteArray"/> for the specified data.
   ''' </summary>
   ''' <param name="arrayToProtect">Byte array to protect.</param>
   Public Sub New(arrayToProtect As Byte())
      RequireNonNull(arrayToProtect, NameOf(arrayToProtect))

      BuildObfuscatedData(arrayToProtect, 0, arrayToProtect.Length)
   End Sub

   ''' <summary>
   ''' reates a New <see cref="ProtectedByteArray"/>for the specified data
   ''' starting from <paramref name="offset"/> with length <paramref name="count"/>.
   ''' </summary>
   ''' <param name="arrayToProtect">Byte array to protect.</param>
   ''' <param name="offset">Offset of the data in the byte array.</param>
   ''' <param name="count">Length of the data in the byte array.</param>
   ''' <exception cref="ArgumentNullException">Thrown if <paramref name="arrayToProtect"/> is <c>Nothing</c>.</exception>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="offset"/> or <paramref name="count"/> do not match <paramref name="arrayToProtect"/>.</exception>
   Public Sub New(arrayToProtect As Byte(), offset As Integer, count As Integer)
      RequireNonNull(arrayToProtect, NameOf(arrayToProtect))

      CheckOffsetAndLength(arrayToProtect, offset, count)

      BuildObfuscatedData(arrayToProtect, offset, count)
   End Sub
#End Region

#Region "Public methods"
   '******************************************************************
   ' Public methods
   '******************************************************************

   ''' <summary>
   ''' Returns the data of the byte array in the clear.
   ''' </summary>
   ''' <returns>The clear data in the byte array.</returns>
   ''' <exception cref="InvalidOperationException">Thrown if this instance has already been disposed of.</exception>
   Public Function GetData() As Byte()
      CheckState()

      Return GetDefuscatedArray()
   End Function

#Region "Overridden Object methods"
   ''' <summary>
   ''' Returns the hash code of this instance.
   ''' </summary>
   ''' <returns>The hash code.</returns>
   ''' <exception cref="InvalidOperationException">Thrown if this instance has already been disposed of.</exception>
   Public Overrides Function GetHashCode() As Integer
      CheckState()

      Return m_ProtectedArray.GetHashCode()
   End Function

   ''' <summary>
   ''' Compares the specified object with this instance.
   ''' </summary>
   ''' <param name="obj">The object to compare.</param>
   ''' <returns><c>True</c>, if the other object is a <see cref="ProtectedByteArray"/> with the same content as this instance, otherwise <c>false</c>.</returns>
   ''' <exception cref="InvalidOperationException">Thrown if this instance has already been disposed of.</exception>
   Public Overrides Function Equals(obj As Object) As Boolean
      CheckState()

      Dim result As Boolean = False

      If TypeOf obj Is ProtectedByteArray Then
         Dim other As ProtectedByteArray = DirectCast(obj, ProtectedByteArray)

         Dim thisClearData As Byte() = GetData()
         Dim otherClearData As Byte() = other.GetData()

         result = ArrayHelper.SecureAreEqual(thisClearData, otherClearData)

         ArrayHelper.Clear(thisClearData)
         ArrayHelper.Clear(otherClearData)
      End If

      Return result
   End Function
#End Region

#Region "Properties"
   ''' <summary>
   ''' Gets the array length.
   ''' </summary>
   ''' <returns>Real length of data stored in this instance.</returns>
   ''' <exception cref="InvalidOperationException">Thrown if this instance has already been disposed of.</exception>
   Public ReadOnly Property Length As Integer
      Get
         CheckState()

         Return m_ProtectedArray.Length
      End Get
   End Property

   ''' <summary>
   ''' Check whether this instance contains valid data.
   ''' </summary>
   ''' <returns><c>True</c>, if this instance has not been disposed of, <c>false</c> otherwise.</returns>
   Public ReadOnly Property IsValid() As Boolean
      Get
         Return m_ProtectedArray.IsValid
      End Get
   End Property
#End Region
#End Region

#Region "Private methods"
   '******************************************************************
   ' Private methods
   '******************************************************************

#Region "Check methods"
   '
   ' Check methods
   '

   ''' <summary>
   ''' Checks whether offset and length are valid for the array.
   ''' </summary>
   ''' <param name="arrayToProtect">Byte array to protect.</param>
   ''' <param name="offset">Offset of the data in the byte array.</param>
   ''' <param name="count">Length of the data in the byte array.</param>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="offset"/> or <paramref name="count"/> are
   ''' incompatible with <paramref name="arrayToProtect"/>.</exception>
   ''' <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="offset"/> or <paramref name="count"/> are
   ''' less than 0 or <paramref name="offset"/> is larger than the length of <paramref name="arrayToProtect"/>.</exception>
   Private Shared Sub CheckOffsetAndLength(arrayToProtect As Byte(), offset As Integer, count As Integer)
      If offset < 0 Then _
         Throw New ArgumentOutOfRangeException(NameOf(offset))

      '
      ' A source array of length 0 has a lower bound of 0 and an upper bound of -1. So we must not check the offset
      ' if the source array has a length of 0.
      '
      If arrayToProtect.Length > 0 AndAlso offset >= arrayToProtect.Length Then _
         Throw New ArgumentOutOfRangeException(NameOf(offset))

      If count < 0 Then _
         Throw New ArgumentOutOfRangeException(NameOf(count))

      If (offset + count) > arrayToProtect.Length Then _
         Throw New ArgumentException("Array is not large enough for count elements from offset")
   End Sub

   ''' <summary>
   ''' Checks whether the protected byte array is in a valid state.
   ''' </summary>
   ''' <exception cref="InvalidOperationException">Thrown if this instance has already been disposed of.</exception>
   Private Sub CheckState()
      If Not m_ProtectedArray.IsValid() Then _
         Throw New InvalidOperationException("ProtectedByteArray has already been disposed of")
   End Sub
#End Region

#Region "Methods for obfuscation and defuscation"
   '
   ' Methods for obfuscation And defuscation
   '

   ''' <summary>
   ''' Build the data structures needed for obfuscation and initialize them.
   ''' </summary>
   ''' <param name="sourceArray">Source array for the obfuscated data.</param>
   Private Sub BuildObfuscatedData(sourceArray As Byte(), offset As Integer, count As Integer)
      m_ProtectedArray = New ShuffledByteArray(sourceArray, offset, count)

      m_Obfuscation = CreateNewObfuscationArray(count)

      StoreInObfuscatedArray(sourceArray)
   End Sub

   ''' <summary>
   ''' Creates a new obfuscation array
   ''' </summary>
   ''' <param name="arrayLength">Length of the New m_Obfuscation array</param>
   ''' <returns>New obfuscation array as a <see cref="ShuffledByteArray"/>.</returns>
   Private Shared Function CreateNewObfuscationArray(arrayLength As Integer) As ShuffledByteArray
      Dim obfuscationSource As Byte() = New Byte(0 To arrayLength - 1) {}

      SecurePseudoRandomNumberGenerator.GetBytes(obfuscationSource)

      Dim result As ShuffledByteArray = New ShuffledByteArray(obfuscationSource)

      ArrayHelper.Clear(obfuscationSource)   ' Clear sensitive data

      Return result
   End Function

   ''' <summary>
   ''' Stores the source xored with the obfuscation bytes in the protected array.
   ''' </summary>
   ''' <param name="source">Byte array to obfuscate.</param>
   Private Sub StoreInObfuscatedArray(source As Byte())
      For i As Integer = 0 To source.Length - 1
         m_ProtectedArray.ElementAt(i) = source(i) Xor m_Obfuscation.ElementAt(i)
      Next
   End Sub

   ''' <summary>
   ''' Xors the obfuscated array to get the clear data.
   ''' </summary>
   ''' <returns>Byte array of clear data.</returns>
   Private Function GetDefuscatedArray() As Byte()
      Dim result As Byte() = New Byte(0 To m_ProtectedArray.Length - 1) {}

      For i As Integer = 0 To result.Length - 1
         result(i) = m_ProtectedArray.ElementAt(i) Xor m_Obfuscation.ElementAt(i)
      Next

      Return result
   End Function
#End Region

#Region "Exception helper methods"
   ''' <summary>
   ''' Check if object is null and throw an exception, if it is.
   ''' </summary>
   ''' <param name="anObject">Object to check.</param>
   ''' <param name="parameterName">Parameter name for exception.</param>
   ''' <exception cref="ArgumentNullException">Thrown when <paramref name="anObject"/> is <c>Nothing</c>.</exception>
   Private Shared Sub RequireNonNull(anObject As Object, parameterName As String)
      If anObject Is Nothing Then _
         Throw New ArgumentNullException(parameterName)
   End Sub
#End Region
#End Region

#Region "IDisposable Support"
   ''' <summary>
   ''' Marker, if disposition of managed resources has already been done.
   ''' </summary>
   Private m_IsDisposed As Boolean = False

   ''' <summary>
   ''' Dispose managed and unmanged resources.
   ''' </summary>
   ''' <param name="disposeManagedResources"><c>true</c>, if managed resource are to be disposed of, <c>false</c>, if not.</param>
   Protected Overridable Sub Dispose(disposeManagedResources As Boolean)
      '
      ' Disposing of resources needs to be synchronized to prevent a race condition.
      '
      SyncLock m_ProtectedArray
         If Not m_IsDisposed Then
            m_IsDisposed = True

            If disposeManagedResources Then
               m_ProtectedArray.Dispose()
               m_Obfuscation.Dispose()
            End If

            ' Free unmanaged resources (unmanaged objects) and override Finalize() below.
            ' Set large fields to null.
         End If
      End SyncLock
   End Sub

   ' Override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
   'Protected Overrides Sub Finalize()
   '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
   '    Dispose(False)
   '    MyBase.Finalize()
   'End Sub

   ''' <summary>
   ''' Dispose of resources.
   ''' </summary>
   ''' <remarks>
   ''' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
   ''' </remarks>
   Public Sub Dispose() Implements IDisposable.Dispose
      Dispose(True)
      ' Uncomment the following line if Finalize() is overridden above.
      ' GC.SuppressFinalize(Me)
   End Sub
#End Region
End Class
