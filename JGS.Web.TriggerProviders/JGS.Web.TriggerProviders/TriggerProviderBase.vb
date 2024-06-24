Imports System.Xml
Imports JGS.DAL
Imports System.Xml.XPath

Public MustInherit Class TriggerProviderBase
   Protected Const EXECUTION_OK As String = "SUCCESS"
   Protected Const EXECUTION_ERROR As String = "ERROR"

#Region "Properties"
   ''' <summary>
   ''' The name of the Trigger Provider
   ''' </summary>
   ''' <value>Sets the name</value>
   ''' <returns>The name of the trigger</returns>
   Public MustOverride Property Name() As String


   Private _connectionString As String
   ''' <summary>
   ''' The connection string used to call DbFunctions
   ''' </summary>
   ''' <value>Sets the connection string</value>
   ''' <returns>The current connection string</returns>
   ''' <remarks></remarks>
   Public Property ConnectionString() As String
      Get
         Return _connectionString
      End Get
      Set(ByVal value As String)
         _connectionString = value
      End Set
   End Property
#End Region

#Region "Base Methods"
   ''' <summary>
   ''' Returns the name and version of the object
   ''' </summary>
   ''' <returns>The name and version (Major.Minor) of the library</returns>
   Public Overrides Function ToString() As String
      Return String.Format("{1} - {2}.{3}", Me.Name, My.Application.Info.Version.Major, My.Application.Info.Version.Minor)
   End Function
#End Region

#Region "Abstract Methods"
   ''' <summary>
   ''' The primary function called from the engine to validate the XML
   ''' </summary>
   ''' <param name="xmlIn">The XmlDocument containing the data to be validated</param>
   ''' <returns>The modified XmlDocument</returns>
   Public MustOverride Function Execute(ByVal xmlIn As XmlDocument) As XmlDocument
#End Region
End Class
