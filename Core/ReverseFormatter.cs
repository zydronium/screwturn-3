﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.IO;

namespace ScrewTurn.Wiki {

	/// <summary>
	/// Implements reverse formatting methods (HTML-&gt;WikiMarkup).
	/// </summary>
	public static class ReverseFormatter {

		/// <summary>
		/// Processes order or unorder lists and sublists.
		/// </summary>
		/// <param name="nodes">The nodes.</param>
		/// <param name="marker">The marker.</param>
		/// <returns>Valid WikiMarkUp text for the lists</returns>
		private static string processList(XmlNodeList nodes, string marker) {
			string result = "";
			string ul = "*";
			string ol = "#";
			foreach(XmlNode node in nodes) {
				if(node.Name.ToString() == "li") {
					foreach(XmlNode child in node.ChildNodes) {
						switch(child.Name.ToString()) {
							case "ol":
								result += processList(child.ChildNodes, marker + ol);
								break;
							case "ul":
								result += processList(child.ChildNodes, marker + ul);
								break;
							default:
								StringReader a = new StringReader(child.OuterXml);
								XmlDocument n = FromHTML((TextReader)a);
								result += marker + " " + processChild(n.ChildNodes) + "\r\n";
								break;
						}
					}
				}
			}
			return result;
		}


		/// <summary>
		/// Processes the image.
		/// </summary>
		/// <param name="node">The node contenent fileName of the image.</param>
		/// <returns>The correct path for wikimarup and image</returns>
		private static string processImage(XmlNode node) {
			string result = "";
			if(node.Attributes.Count != 0) {
				foreach(XmlAttribute attName in node.Attributes) {
					if(attName.Name.ToString() == "src") {
						string[] path = attName.Value.ToString().Split('=');
						result += "{" + "UP(" + path[1].Split('&')[0] + ")}" + path[2];
					}
				}
			}
			return result;
		}

		/// <summary>
		/// Processes the child Image.
		/// </summary>
		/// <param name="nodes">Nodelist from an image.</param>
		/// <returns>The correct WikiMarkup for the images </returns>
		private static string processChildImage(XmlNodeList nodes) {
			string image = "";
			string p = "";
			string url = "";
			string result = "";
			foreach(XmlNode node in nodes) {
				if(node.Name.ToLowerInvariant() == "img")
					image += processImage(node);
				else if(node.Name.ToLowerInvariant() == "p") {
					p += "|" + processChild(node.ChildNodes) + "|";
				}
				else if(node.Name.ToLowerInvariant() == "a") {
					string link = "";
					string target = "";

					if(node.Attributes.Count != 0) {
						XmlAttributeCollection attribute = node.Attributes;
						foreach(XmlAttribute attName in attribute) {
							if(attName.Value.ToString() == "_blank")
								target += "^";
							if(attName.Name.ToString() == "href")
								link += attName.Value.ToString();
						}
					}
					image += processImage(node.LastChild);
					url = "|" + target + link;
				}
			}
			result = p + image + url;
			return result;
		}

		/// <summary>
		/// Processes the child.
		/// </summary>
		/// <param name="nodes">A XmlNodeList .</param>
		/// <returns>The corrispondent WikiMarkup Text</returns>
		private static string processChild(XmlNodeList nodes) {
			string result = "";
			foreach(XmlNode node in nodes) {
				bool anchor = false;
				if(node.NodeType == XmlNodeType.Text) {
					result += node.Value;
					//string result = "";
				}
				else {
					switch(node.Name.ToLowerInvariant()) {
						case "html":
							result += processChild(node.ChildNodes);
							break;
						case "b":
						case "strong":
							result += ("'''" + processChild(node.ChildNodes) + "'''");
							break;
						case "s":
							result += ("--" + processChild(node.ChildNodes) + "--");
							break;
						case "em":
						case "i":
							result += ("''" + processChild(node.ChildNodes) + "''");
							break;
						case "u":
							result += ("__" + processChild(node.ChildNodes) + "__");
							break;
						//break;
						case "h1":
							result += ("==" + processChild(node.ChildNodes) + "==");
							break;
						//break;
						case "h2":
							result += ("===" + processChild(node.ChildNodes) + "===");
							break;
						//break;
						case "h3":
							result += ("====" + processChild(node.ChildNodes) + "====");
							break;
						//break;
						case "h4":
							result += ("=====" + processChild(node.ChildNodes) + "=====");
							break;
						case "pre":
							result += ("@@" + node.InnerText.ToString() + "@@");
							break;
						case "code":
							result += ("{{" + processChild(node.ChildNodes) + "}}");
							break;
						case "hr":
						case "hr /":
							result += ("----" + processChild(node.ChildNodes));
							break;
						case "\t":
							result += (":" + processChild(node.ChildNodes));
							break;
						case "éé":
							result += ("~~~~" + processChild(node.ChildNodes));
							break;
						case "span":
							if(node.Attributes.Count != 0) {
								XmlAttributeCollection attribute = node.Attributes;
								foreach(XmlAttribute attName in attribute) {
									if(attName.Value.ToString() == "italic")
										result += "''" + processChild(node.ChildNodes) + "''";
								}
							}
							break;
						case "\n":
						case "br":
							result += ("\r\n" + processChild(node.ChildNodes));
							break;
						case "table":
							string image = "";
							bool isImage = false;
							foreach(XmlAttribute attName in node.Attributes) {
								if(attName.Value.ToString() == "imageauto") {
									isImage = true;
									image += "[imageauto|" + processChild(node.ChildNodes) + "]\r\n";
								}
							}
							if(isImage) {
								result += image;
								break;
							}
							else result += processChild(node.ChildNodes);
							break;
						case "tbody":
							result += processChild(node.ChildNodes);
							break;
						case "tr":
							result += processChild(node.ChildNodes);
							break;
						case "td":
							result += processChild(node.ChildNodes);
							break;
						case "ol":
							result += processList(node.ChildNodes, "#") + "\r\n";
							break;
						case "ul":
							result += processList(node.ChildNodes, "*") + "\r\n";
							break;
						case "li":
							result += processChild(node.ChildNodes);
							break;
						case "sup":
							result += ("<sup>" + processChild(node.ChildNodes) + "</sup>");
							break;
						case "sub":
							result += ("<sub>" + processChild(node.ChildNodes) + "</sub>");
							break;
						case "p":
							if(node.Attributes.Count != 0) {
								XmlAttributeCollection attribute = node.Attributes;
								foreach(XmlAttribute attName in attribute) {
									if(attName.Value.ToString() == "imagedescription")
										result += "";
								}
							}
							break;
						case "div":
							if(node.Attributes.Count != 0) {
								XmlAttributeCollection attribute = node.Attributes;
								foreach(XmlAttribute attName in attribute) {
									if(attName.Value.ToString() == "box") {
										result += "(((" + processChild(node.ChildNodes) + ")))\r\n";
									}
									if(attName.Value.ToString() == "imageleft") {
										result += "[imageleft" + processChildImage(node.ChildNodes) + "]\r\n";
									}
									if(attName.Value.ToString() == "imageright")
										result += "[imageright" + processChildImage(node.ChildNodes) + "]\r\n";
									if(attName.Value.ToString() == "imageauto")
										result += "[imageauto" + processChildImage(node.ChildNodes) + "]\r\n";
								}
							}
							else
								result += (processChild(node.ChildNodes) + "\r\n");
							break;

						case "img":
							string description = "";
							bool hasClass = false;
							bool isLink = false;
							if(node.ParentNode != null)
								if(node.ParentNode.Name.ToLowerInvariant().ToString() == "a")
									isLink = true;
							if(node.Attributes.Count != 0) {
								foreach(XmlAttribute attName in node.Attributes) {
									if(attName.Name.ToString() == "alt")
										description = attName.Value.ToString();
									if(attName.Name.ToString() == "class")
										hasClass = true;
								}
							}
							if((!hasClass) && (!isLink))
								result += "[image|" + description + "|" + processImage(node) + "]\r\n";
							else if((!hasClass) && (isLink))
								result += "[image|" + description + "|" + processImage(node);
							else
								result += description + "|" + processImage(node);
							break;

						case "a":
							bool isTable = false;
							string link = "";
							string target = "";
							string title = "";
							bool childImg = false;
							if(node.FirstChild != null) {
								if(node.FirstChild.Name.ToLowerInvariant() == "img")
									childImg = true;
							}
							if(node.ParentNode.Name.ToLowerInvariant() == "td")
								isTable = true;
							if(node.Attributes.Count != 0) {
								XmlAttributeCollection attribute = node.Attributes;
								foreach(XmlAttribute attName in attribute) {
									if(attName.Name.ToString() != "id".ToLowerInvariant()) {
										if(attName.Value.ToString() == "_blank")
											target += "^";
										if(attName.Name.ToString() == "href")
											link += attName.Value.ToString();
										if(attName.Name.ToString() == "title")
											title += attName.Value.ToString();
									}
									else {
										anchor = true;
										result += "[anchor|#" + attName.Value.ToString().ToLowerInvariant() + "]" + processChild(node.ChildNodes);
										break;
									}
								}

								if((!anchor) && (!isTable) && (!childImg))
									if(title != link)
										result += "[" + target + link + "|" + processChild(node.ChildNodes) + "]";
									else
										result += "[" + target + link + "|" + processChild(node.ChildNodes) + "]";
								if((!anchor) && (isTable))
									result += processChild(node.ChildNodes) + "|" + target + link;
								if((!anchor) && (childImg) && (!isTable))
									result += processChild(node.ChildNodes) + "|" + target + link + "]\r\n";
							}
							break;

						default:
							result += (node.OuterXml);
							break;
					}
				}
			}
			return result;
		}

		/// <summary>
		/// Froms the HTML.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <returns>valid XML Document</returns>
		private static XmlDocument FromHTML(TextReader reader) {

			// setup SgmlReader
			Sgml.SgmlReader sgmlReader = new Sgml.SgmlReader();
			sgmlReader.DocType = "HTML";
			sgmlReader.WhitespaceHandling = WhitespaceHandling.All;

			sgmlReader.CaseFolding = Sgml.CaseFolding.ToLower;
			sgmlReader.InputStream = reader;

			// create document
			XmlDocument doc = new XmlDocument();
			doc.PreserveWhitespace = true;
			doc.XmlResolver = null;
			doc.Load(sgmlReader);
			return doc;
		}

		/// <summary>
		/// Reverse formats HTML content into WikiMarkup.
		/// </summary>
		/// <param name="html">The input HTML.</param>
		/// <returns>The corresponding WikiMarkup.</returns>
		public static string ReverseFormat(string html) {

			StringReader strReader = new StringReader(html);
			XmlDocument x = FromHTML((TextReader)strReader);
			return processChild(x.FirstChild.ChildNodes);
		}
	}
}