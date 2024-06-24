Imports System.Xml
Imports JGS.DAL
Imports Oracle.DataAccess.Client

''' <summary>
''' Helper functions
''' </summary>
''' <remarks></remarks>
Public Module Functions
   ''' <summary>
   ''' Write a line to the debug output
   ''' </summary>
   ''' <param name="message">The message to write</param>
   ''' <remarks>Provided to simplify conversion from Oracle</remarks>
   Public Sub DebugOut(ByVal message As String)
      Debug.WriteLine(message)
   End Sub

   ''' <summary>
   ''' Checks to see wether a specified node exists and is not empty.
   ''' </summary>
   ''' <param name="xmlIn">The XmlDocument to search</param>
   ''' <param name="xPath">The ZPath to the node to test</param>
   ''' <returns>True if the node exists and contains a value, false otherwise.</returns>
   ''' <remarks></remarks>
   Public Function IsNull(ByVal xmlIn As XmlDocument, ByVal xPath As String) As Boolean
      If Not NodeExists(xmlIn, xPath) Then
         Return True
      End If
      If ExtractValue(xmlIn, xPath) = Nothing Then
         Return True
      Else
         Return False
      End If
   End Function

   ''' <summary>
   ''' Returns the first value found at the specified XPath
   ''' </summary>
   ''' <param name="xmlIn">The XmlDocument to search</param>
   ''' <param name="xPath">The XPath to the desired value</param>
   ''' <returns>The value at the specified XPath. If the node is not found or is empty, returns null.</returns>
   Public Function ExtractValue(ByVal xmlIn As XmlDocument, ByVal xPath As String) As String
      If Not NodeExists(xmlIn, xPath) Then
         Return Nothing
      End If
      Try
         Dim ns As XmlNamespaceManager
         Dim node As XmlNode

         ns = New XmlNamespaceManager(xmlIn.NameTable)
         node = xmlIn.SelectSingleNode(xPath)
         If node.InnerText = String.Empty Then
            Return Nothing
         End If
         Return node.InnerText
      Catch ex As Exception
         Return Nothing
      End Try
   End Function

   ''' <summary>
   ''' Update the selected XPath
   ''' </summary>
   ''' <param name="xmlIn">The XmlDocument to update</param>
   ''' <param name="xPath">The XPath to update</param>
   ''' <param name="value">The new value for the node</param>
   ''' <exception cref="ArgumentException">Throws an ArgumentException if the node does not exist.</exception>
   ''' <remarks></remarks>
   Public Sub UpdateXml(ByRef xmlIn As XmlDocument, ByVal xPath As String, ByVal value As String)
      If Not NodeExists(xmlIn, xPath) Then
         Throw New ArgumentException("XML Node " & xPath & " does not exist")
      End If
      Dim ns As XmlNamespaceManager
      Dim node As XmlNode

      ns = New XmlNamespaceManager(xmlIn.NameTable)
      node = xmlIn.SelectSingleNode(xPath)
      node.InnerText = value
   End Sub

   ''' <summary>
   ''' Determines wether the XPath node exists
   ''' </summary>
   ''' <param name="xmlIn">The XmlDocument to search</param>
   ''' <param name="xPath">The XPath to test</param>
   ''' <returns>True if there is one or more nodes of that type, false otherwise</returns>
   Public Function NodeExists(ByVal xmlIn As XmlDocument, ByVal xPath As String) As Boolean
      Try
         Dim ns As XmlNamespaceManager
         Dim nodes As XmlNodeList

         ns = New XmlNamespaceManager(xmlIn.NameTable)
         nodes = xmlIn.SelectNodes(xPath)
         Return nodes.Count > 0
      Catch ex As Exception
         Return False
      End Try
   End Function

   ''' <summary>
   ''' Execute an Oracle scalar function and return the results as a string
   ''' </summary>
   ''' <param name="schema">The schema of the function</param>
   ''' <param name="package">The name of the package containing the function</param>
   ''' <param name="functionName">The name of the function to execute</param>
   ''' <param name="params">The array of parameters to the function</param>
   ''' <returns>The string result, nothing on failure</returns>
   ''' <remarks></remarks>
   Public Function DbFetch(ByVal connectionString As String, _
                           ByVal schema As String, ByVal package As String, ByVal functionName As String, _
                                  ByVal params As List(Of OracleParameter)) As String
      Try
         Dim result As String = String.Empty
            params.Insert(0, New OracleParameter("result", OracleDbType.Varchar2, 5000, result, ParameterDirection.ReturnValue))
            ODPNETHelper.ExecuteScalar(connectionString, CommandType.StoredProcedure, _
            schema & "." & package & "." & functionName, params.ToArray())
            If params(0).Value.ToString().ToUpper() = "NULL" Then
                Return Nothing
            End If
            result = params(0).Value.ToString()
         Return result
      Catch ex As Exception
         Return Nothing
      End Try
   End Function

   ''' <summary>
   ''' Returns the first value found at the specified XPath
   ''' </summary>
   ''' <param name="xmlIn">The XmlDocument to search</param>
   ''' <param name="xPath">The XPath to the desired value</param>
   ''' <returns>The value at the specified XPath. If the node is not found or is empty, returns null.</returns>
   Public Function ExtractValueByName(ByVal xmlIn As XmlDocument, ByVal xPath As String, ByVal Name As String) As String
      If Not NodeExists(xmlIn, xPath) Then
         Return Nothing
      End If
      Try
         Dim ns As XmlNamespaceManager
         Dim node As XmlNode

         ns = New XmlNamespaceManager(xmlIn.NameTable)
         node = xmlIn.SelectSingleNode(xPath)
         If node.InnerText = String.Empty Then
            Return Nothing
         End If
         Return node.InnerText
      Catch ex As Exception
         Return Nothing
      End Try
   End Function

   ''' <summary>
   ''' Executes a set of simple scalar queries
   ''' </summary>
   ''' <param name="connectionString">The connection string to the data server</param>
   ''' <param name="queries">The queries to execute</param>
   ''' <remarks></remarks>
   Public Sub GetMultipleDbValues(ByVal connectionString As String, _
                                       ByVal queries As Dictionary(Of String, OracleQuickQuery))
      For Each query As OracleQuickQuery In queries.Values
         With query
            Dim result As String = String.Empty
            Dim dsResult As DataSet = ODPNETHelper.ExecuteDataset(connectionString, CommandType.Text, query.ToString())
            If dsResult.Tables.Count > 0 Then
               If dsResult.Tables(0).Rows.Count > 0 Then
                  result = dsResult.Tables(0).Rows(0).Item(0).ToString()
               End If
            End If
            .Result = result
         End With
      Next
   End Sub
End Module
