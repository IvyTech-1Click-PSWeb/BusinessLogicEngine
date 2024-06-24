using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Xml;
using System.Runtime.CompilerServices;
using System.ComponentModel;

/// <summary>
/// Helper functions
/// </summary>
/// <remarks></remarks>

namespace JGS.Shared
{
	/// <summary>
	/// Useful extension methods for handling XmlDocuments and Packages
	/// </summary>
	public static class Extensions
	{
		#region "Extension Methods"
		/// <summary>
		/// Loads an XmlElement into the specified XmlDocument
		/// </summary>
		/// <param name="document">The document to load into</param>
		/// <param name="xmlElement">The element to load</param>
		public static void LoadXmlElement(this XmlDocument document, XmlElement xmlElement)
		{
			XmlNode node = document.ImportNode(xmlElement, true);
			document.AppendChild(node);
		}

		/// <summary>
		/// Returns the value at the specified XPath
		/// </summary>
		/// <typeparam name="T">The return type</typeparam>
		/// <param name="document">An XmlDocument to search</param>
		/// <param name="xPath">The XPath to return</param>
		/// <returns>The value at the specified XPath, default(T) if the node is empty or non-existent</returns>
		public static T GetValue<T>(this XmlNode document, string xPath)
			where T : IConvertible
		{
			return document.GetValue<T>(xPath, default(T));
		}

		/// <summary>
		/// Returns the value at the specified XPath
		/// </summary>
		/// <typeparam name="T">The return type</typeparam>
		/// <param name="document">An XmlDocument to search</param>
		/// <param name="xPath">The XPath to return</param>
		/// <param name="defaultValue">The value to return if the node is empty or non-existent</param>
		/// <returns>The value at the specified XPath, <see cref="defaultValue"/> if the node is empty or non-existent</returns>
		public static T GetValue<T>(this XmlNode document, string xPath, T defaultValue)
			where T : IConvertible
		{
			if(!document.NodeExists(xPath) || document.NodeIsNullOrEmpty(xPath))
			{
				return defaultValue;
			}

			return (T)Convert.ChangeType(document.SelectSingleNode(xPath).InnerText, typeof(T));
		}

		/// <summary>
		/// Sets the value at the specified XPath, creating it if it does not exist
		/// </summary>
		/// <param name="document">An XmlDocument to search</param>
		/// <param name="xPath">The XPath to set</param>
		/// <param name="value">The value to set</param>
		public static void SetValue(this XmlNode document, string xPath, string value)
		{
			document.SetValue(xPath, value, true);
		}

		/// <summary>
		/// Sets the value at the specified XPath
		/// </summary>
		/// <param name="document">An XmlDocument to search</param>
		/// <param name="xPath">The XPath to set</param>
		/// <param name="value">The value to set</param>
		/// <param name="create">Should the node be created if it does not exist</param>
		public static void SetValue(this XmlNode document, string xPath, string value, bool create) {
			if(!document.NodeExists(xPath))
			{
				if(create)
				{
					document.CreateXPathNode(xPath);
				}
			} 

			document.SelectSingleNode(xPath).InnerText = value;
		}

		/// <summary>
		/// Indicates whether the specified XPath node object exists in the document
		/// </summary>
		/// <param name="document">An XmlDocument to search</param>
		/// <param name="xPath">The XPath to evaluate</param>
		/// <returns>rue if there is one or more nodes of that type, false otherwise</returns>
		public static bool NodeExists(this XmlNode document, string xPath)
		{
			return document.SelectSingleNode(xPath) != null;
		}

		/// <summary>
		/// Indicates whether the node at the specified XPath is Empty
		/// </summary>
		/// <param name="document">An XmlDocument to search</param>
		/// <param name="xPath">The path to evaluate</param>
		/// <returns>True if the node is empty</returns>
		/// <exception cref="ArgumentException">The specified XPath does not exist in the document</exception>
		public static bool NodeIsEmpty(this XmlNode document, string xPath)
		{
			if(!document.NodeExists(xPath))
			{
				throw new ArgumentException("Node not found", "xPath");
			}
			return (document.GetValue<string>(xPath) == string.Empty);
		}

		/// <summary>
		/// Indicates whether the specified XPath node object is null or an Empty string.
		/// </summary>
		/// <param name="document">An XmlDocument to search</param>
		/// <param name="xPath">The XPath to evaluate</param>
		/// <returns></returns>
		public static bool NodeIsNullOrEmpty(this XmlNode document, string xPath)
		{
			if(!document.NodeExists(xPath)) { return true; }
			return string.IsNullOrEmpty(document.SelectSingleNode(xPath).InnerText);
		}

		/// <summary>
		/// Changes an object to the specified type
		/// </summary>
		/// <typeparam name="T">The type to return</typeparam>
		/// <param name="value">THe object to change</param>
		/// <returns>The object as type T</returns>
		/// <remarks>The returned object must be casted before use</remarks>
		public static object ChangeTypeTo<T>(this T value)
		{
			if(value == null) { return null; }

			Type underlyingType = typeof(T);
			if(underlyingType == null) { throw new ArgumentException("Invalid type received", "value"); }

			if(underlyingType.IsGenericType && underlyingType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
			{
				NullableConverter converter = new NullableConverter(underlyingType);
				underlyingType = converter.UnderlyingType;
			}

			if(underlyingType == typeof(Guid)) { return new Guid(value.ToString()); }

			Type objType = value.GetType();
			bool objTypeAssignable2typeT = underlyingType.IsAssignableFrom(objType);
			return objTypeAssignable2typeT
						 ? Convert.ChangeType(value, underlyingType)
						 : Convert.ChangeType(value.ToString(), underlyingType);
		}

		/// <summary>
		/// Changes an object to the specified type
		/// </summary>
		/// <typeparam name="T">The type to return</typeparam>
		/// <param name="value">THe object to change</param>
		/// <returns>The object as type T</returns>
		/// /// <remarks>The returned object must be casted before use</remarks>
		public static object ChangeTypeTo<T>(this object value)
		{
			if(value == null) { return null; }

			Type underlyingType = typeof(T);
			if(underlyingType == null) { throw new ArgumentException("Invalid type received", "value"); }

			if(underlyingType.IsGenericType && underlyingType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
			{
				NullableConverter converter = new NullableConverter(underlyingType);
				underlyingType = converter.UnderlyingType;
			}

			if(underlyingType == typeof(Guid)) { return new Guid(value.ToString()); }

			Type objType = value.GetType();
			bool objTypeAssignable2typeT = underlyingType.IsAssignableFrom(objType);
			return objTypeAssignable2typeT
						 ? Convert.ChangeType(value, underlyingType)
						 : Convert.ChangeType(value.ToString(), underlyingType);
		}

		/// <summary>
		/// Create a node at the specified XPath
		/// </summary>
		/// <param name="document">An XmlDocument to modify</param>
		/// <param name="xpath">The XPath to create</param>
		/// <returns>The newly created node</returns>
		public static XmlNode CreateXPathNode(this XmlNode node, string xpath)
		{
			XmlDocument document = node.OwnerDocument == null?(XmlDocument)node:node.OwnerDocument;
			
			XmlNode rootNode = node;
			XmlNode parentNode;
			if (xpath.StartsWith("/"))
			{
				rootNode = document;
			}
			parentNode = rootNode;

			if(document != null && !string.IsNullOrEmpty(xpath))
			{
				List<string> partsOfXPath = new List<string>(xpath.Split('/'));
				string nodeName = partsOfXPath[partsOfXPath.Count - 1];
				partsOfXPath.RemoveAt(partsOfXPath.Count - 1);

				string xPathSoFar = string.Empty;

				foreach(string xPathElement in partsOfXPath)
				{
					if(string.IsNullOrEmpty(xPathElement))
						continue;

					xPathSoFar += "/" + xPathElement.Trim();

					XmlNode childNode = rootNode.SelectSingleNode(xPathSoFar);
					
					if(childNode == null)
					{
						childNode = document.CreateElement(xPathElement);
					}

					parentNode.AppendChild(childNode);

					parentNode = childNode;
				}
				//Create the new node element and add it to the tree
				XmlNode newNode = document.CreateElement(nodeName);
				parentNode.AppendChild(newNode);
				return newNode;
			}
			return null;
		}

		/// <summary>
		/// Returns the specified field from the a data row
		/// </summary>
		/// <typeparam name="T">The type to return</typeparam>
		/// <param name="row">The data row</param>
		/// <param name="key">The field name to return</param>
		/// <returns>The value of the specified field, default(T) is the field is not found</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if the field is not contained in the row</exception>
		public static T GetItem<T>(this DataRow row, string key)
		{
			if(row.Table.Columns.Contains(key))
			{
				if(typeof(T) == typeof(string) && !row.IsEmptyString(key))
				{
					return (T)row[key].ChangeTypeTo<T>();
				}
				if(row.IsNull(key) || row.IsEmptyString(key))
				{
					return default(T);
				}
				return (T)ChangeTypeTo<T>(row[key]);
			}
			throw new ArgumentOutOfRangeException(key, row, "does not contain column");
		}

		/// <summary>
		/// Indicates whether the specified field in the data row is an empty string
		/// </summary>
		/// <param name="row">The data row to query</param>
		/// <param name="key">The name of the field to return</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if the field is not contained in the row</exception>
		/// <returns>A boolean indicating whether the field is an empty string</returns>
		public static bool IsEmptyString(this DataRow row, string key)
		{
			if(row.Table.Columns.Contains(key))
			{
				return !row.IsNull(key) && row[key].ToString() == string.Empty;
			}
			throw new ArgumentOutOfRangeException(key, row, "does not contain column");
		}
		#endregion
	}
}
