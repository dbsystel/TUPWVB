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
' Version: 1.0.0
'
' Change history:
'    2020-04-29: V1.0.0: Created.
'

Imports System.Runtime.Serialization

''' <summary>
''' Exception to indicate that data was tampered with.
''' </summary>
<Serializable>
Public Class DataIntegrityException : Inherits Exception
   Public Sub New()
      MyBase.New()
   End Sub

   Public Sub New(message As String)
      MyBase.New(message)
   End Sub

   Public Sub New(message As String, innerException As Exception)
      MyBase.New(message, innerException)
   End Sub

   Protected Sub New(info As SerializationInfo, context As StreamingContext)
      MyBase.New(info, context)
   End Sub
End Class
