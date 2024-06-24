namespace JGS.Web.DellTriggerProviders.Test
{
	partial class MainForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if(disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.HeaderPanel = new System.Windows.Forms.Panel();
			this.TriggerCombo = new System.Windows.Forms.ComboBox();
			this.TriggerLabel = new System.Windows.Forms.Label();
			this.LoadButton = new System.Windows.Forms.Button();
			this.RunButton = new System.Windows.Forms.Button();
			this.ChooseXmlInButton = new System.Windows.Forms.Button();
			this.XmlInText = new System.Windows.Forms.TextBox();
			this.XmlInLabel = new System.Windows.Forms.Label();
			this.XmlComparisonContainer = new System.Windows.Forms.SplitContainer();
			this.XmlInEditor = new ScintillaNet.Scintilla();
			this.XmlOutEditor = new ScintillaNet.Scintilla();
			this.XmlInOpen = new System.Windows.Forms.OpenFileDialog();
			this.HeaderPanel.SuspendLayout();
			this.XmlComparisonContainer.Panel1.SuspendLayout();
			this.XmlComparisonContainer.Panel2.SuspendLayout();
			this.XmlComparisonContainer.SuspendLayout();
			this.XmlInEditor.BeginInit();
			this.XmlOutEditor.BeginInit();
			this.SuspendLayout();
			// 
			// HeaderPanel
			// 
			this.HeaderPanel.Controls.Add(this.TriggerCombo);
			this.HeaderPanel.Controls.Add(this.TriggerLabel);
			this.HeaderPanel.Controls.Add(this.LoadButton);
			this.HeaderPanel.Controls.Add(this.RunButton);
			this.HeaderPanel.Controls.Add(this.ChooseXmlInButton);
			this.HeaderPanel.Controls.Add(this.XmlInText);
			this.HeaderPanel.Controls.Add(this.XmlInLabel);
			this.HeaderPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.HeaderPanel.Location = new System.Drawing.Point(0, 0);
			this.HeaderPanel.Name = "HeaderPanel";
			this.HeaderPanel.Size = new System.Drawing.Size(692, 100);
			this.HeaderPanel.TabIndex = 0;
			// 
			// TriggerCombo
			// 
			this.TriggerCombo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
			this.TriggerCombo.FormattingEnabled = true;
			this.TriggerCombo.Items.AddRange(new object[] {
            "HTCTriggerProvider","TRG_SCRAP_APPROVAL","TRG_B2B_VALIDATIONS","TRG_PXL_VALIDATIONS","TRG_SET_FLEXFIELDS","TRG_SCRAP_OEMWARR_UPD","TRG_SCRAP_3FFS_UPD","TRG_SCRAP_FA_VALIDATIONS","TRG_CP_PARTNUMSPECCHARCHECK","TRG_CP_PNNOTEQUALDELLPN","TRG_CP_PNISEQUALDELLPN","TRG_CP_UPDATE_FFS_XML","TRG_SCRAP_OEMWARR_UPD_XML","TRG_CP_UPDATE_FFS_XML_WC"});
			this.TriggerCombo.Location = new System.Drawing.Point(179, 67);
			this.TriggerCombo.Name = "TriggerCombo";
			this.TriggerCombo.Size = new System.Drawing.Size(343, 24);
			this.TriggerCombo.TabIndex = 6;
			// 
			// TriggerLabel
			// 
			this.TriggerLabel.AutoSize = true;
			this.TriggerLabel.Location = new System.Drawing.Point(12, 70);
			this.TriggerLabel.Name = "TriggerLabel";
			this.TriggerLabel.Size = new System.Drawing.Size(161, 16);
			this.TriggerLabel.TabIndex = 5;
			this.TriggerLabel.Text = "Choose Trigger to Test";
			this.TriggerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// LoadButton
			// 
			this.LoadButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.LoadButton.AutoSize = true;
			this.LoadButton.Location = new System.Drawing.Point(528, 38);
			this.LoadButton.Name = "LoadButton";
			this.LoadButton.Size = new System.Drawing.Size(148, 26);
			this.LoadButton.TabIndex = 4;
			this.LoadButton.Text = "Load XML Input File";
			this.LoadButton.UseVisualStyleBackColor = true;
			this.LoadButton.Click += new System.EventHandler(this.LoadButton_Click);
			// 
			// RunButton
			// 
			this.RunButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.RunButton.AutoSize = true;
			this.RunButton.Location = new System.Drawing.Point(528, 68);
			this.RunButton.Name = "RunButton";
			this.RunButton.Size = new System.Drawing.Size(152, 26);
			this.RunButton.TabIndex = 3;
			this.RunButton.Text = "Run Test";
			this.RunButton.UseVisualStyleBackColor = true;
			this.RunButton.Click += new System.EventHandler(this.RunButton_Click);
			// 
			// ChooseXmlInButton
			// 
			this.ChooseXmlInButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ChooseXmlInButton.AutoSize = true;
			this.ChooseXmlInButton.Location = new System.Drawing.Point(643, 7);
			this.ChooseXmlInButton.Name = "ChooseXmlInButton";
			this.ChooseXmlInButton.Size = new System.Drawing.Size(33, 26);
			this.ChooseXmlInButton.TabIndex = 2;
			this.ChooseXmlInButton.Text = "...";
			this.ChooseXmlInButton.UseVisualStyleBackColor = true;
			this.ChooseXmlInButton.Click += new System.EventHandler(this.ChooseXmlInButton_Click);
			// 
			// XmlInText
			// 
			this.XmlInText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
			this.XmlInText.Location = new System.Drawing.Point(179, 9);
			this.XmlInText.Name = "XmlInText";
			this.XmlInText.Size = new System.Drawing.Size(458, 23);
			this.XmlInText.TabIndex = 1;
			// 
			// XmlInLabel
			// 
			this.XmlInLabel.Location = new System.Drawing.Point(36, 10);
			this.XmlInLabel.Name = "XmlInLabel";
			this.XmlInLabel.Size = new System.Drawing.Size(137, 23);
			this.XmlInLabel.TabIndex = 0;
			this.XmlInLabel.Text = "XML Input File";
			this.XmlInLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// XmlComparisonContainer
			// 
			this.XmlComparisonContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.XmlComparisonContainer.Location = new System.Drawing.Point(0, 100);
			this.XmlComparisonContainer.Margin = new System.Windows.Forms.Padding(0);
			this.XmlComparisonContainer.Name = "XmlComparisonContainer";
			// 
			// XmlComparisonContainer.Panel1
			// 
			this.XmlComparisonContainer.Panel1.Controls.Add(this.XmlInEditor);
			// 
			// XmlComparisonContainer.Panel2
			// 
			this.XmlComparisonContainer.Panel2.Controls.Add(this.XmlOutEditor);
			this.XmlComparisonContainer.Size = new System.Drawing.Size(692, 362);
			this.XmlComparisonContainer.SplitterDistance = 344;
			this.XmlComparisonContainer.SplitterIncrement = 10;
			this.XmlComparisonContainer.TabIndex = 1;
			// 
			// XmlInEditor
			// 
			this.XmlInEditor.ConfigurationManager.Language = "xml";
			this.XmlInEditor.Dock = System.Windows.Forms.DockStyle.Fill;
			this.XmlInEditor.IsBraceMatching = true;
			this.XmlInEditor.IsReadOnly = true;
			this.XmlInEditor.Lexing.Lexer = ScintillaNet.Lexer.Xml;
			this.XmlInEditor.Lexing.LexerName = "xml";
			this.XmlInEditor.Lexing.LineCommentPrefix = "";
			this.XmlInEditor.Lexing.StreamCommentPrefix = "";
			this.XmlInEditor.Lexing.StreamCommentSufix = "";
			this.XmlInEditor.LineWrap.Mode = ScintillaNet.WrapMode.Word;
			this.XmlInEditor.LineWrap.VisualFlags = ((ScintillaNet.WrapVisualFlag)((ScintillaNet.WrapVisualFlag.End | ScintillaNet.WrapVisualFlag.Start)));
			this.XmlInEditor.Location = new System.Drawing.Point(0, 0);
			this.XmlInEditor.Margins.Left = 20;
			this.XmlInEditor.Name = "XmlInEditor";
			this.XmlInEditor.Size = new System.Drawing.Size(344, 362);
			this.XmlInEditor.Styles.BraceBad.FontName = "Verdana";
			this.XmlInEditor.Styles.BraceLight.FontName = "Verdana";
			this.XmlInEditor.Styles.ControlChar.FontName = "Verdana";
			this.XmlInEditor.Styles.Default.FontName = "Verdana";
			this.XmlInEditor.Styles.IndentGuide.FontName = "Verdana";
			this.XmlInEditor.Styles.LastPredefined.FontName = "Verdana";
			this.XmlInEditor.Styles.LineNumber.FontName = "Verdana";
			this.XmlInEditor.Styles.Max.FontName = "Verdana";
			this.XmlInEditor.TabIndex = 0;
			this.XmlInEditor.Scroll += new System.EventHandler<System.Windows.Forms.ScrollEventArgs>(this.XmlInEditor_Scroll);
			// 
			// XmlOutEditor
			// 
			this.XmlOutEditor.ConfigurationManager.Language = "xml";
			this.XmlOutEditor.Dock = System.Windows.Forms.DockStyle.Fill;
			this.XmlOutEditor.IsReadOnly = true;
			this.XmlOutEditor.LineWrap.Mode = ScintillaNet.WrapMode.Word;
			this.XmlOutEditor.LineWrap.VisualFlags = ((ScintillaNet.WrapVisualFlag)((ScintillaNet.WrapVisualFlag.End | ScintillaNet.WrapVisualFlag.Start)));
			this.XmlOutEditor.Location = new System.Drawing.Point(0, 0);
			this.XmlOutEditor.Margin = new System.Windows.Forms.Padding(0);
			this.XmlOutEditor.Margins.Left = 20;
			this.XmlOutEditor.Name = "XmlOutEditor";
			this.XmlOutEditor.Size = new System.Drawing.Size(344, 362);
			this.XmlOutEditor.Styles.BraceBad.FontName = "Verdana";
			this.XmlOutEditor.Styles.BraceLight.FontName = "Verdana";
			this.XmlOutEditor.Styles.ControlChar.FontName = "Verdana";
			this.XmlOutEditor.Styles.Default.FontName = "Verdana";
			this.XmlOutEditor.Styles.IndentGuide.FontName = "Verdana";
			this.XmlOutEditor.Styles.LastPredefined.FontName = "Verdana";
			this.XmlOutEditor.Styles.LineNumber.FontName = "Verdana";
			this.XmlOutEditor.Styles.Max.FontName = "Verdana";
			this.XmlOutEditor.TabIndex = 0;
			this.XmlOutEditor.Scroll += new System.EventHandler<System.Windows.Forms.ScrollEventArgs>(this.XmlOutEditor_Scroll);
			// 
			// XmlInOpen
			// 
			this.XmlInOpen.DefaultExt = "xml";
			this.XmlInOpen.Filter = "Xml Files|*.xml|All Files|*.*";
			this.XmlInOpen.RestoreDirectory = true;
			this.XmlInOpen.SupportMultiDottedExtensions = true;
			this.XmlInOpen.Title = "Choose input XML file";
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.ClientSize = new System.Drawing.Size(692, 462);
			this.Controls.Add(this.XmlComparisonContainer);
			this.Controls.Add(this.HeaderPanel);
			this.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Margin = new System.Windows.Forms.Padding(4);
			this.Name = "MainForm";
			this.Text = "Dell Trigger Providers Test Harness";
			this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
			this.HeaderPanel.ResumeLayout(false);
			this.HeaderPanel.PerformLayout();
			this.XmlComparisonContainer.Panel1.ResumeLayout(false);
			this.XmlComparisonContainer.Panel2.ResumeLayout(false);
			this.XmlComparisonContainer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.XmlInEditor)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.XmlOutEditor)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel HeaderPanel;
		private System.Windows.Forms.SplitContainer XmlComparisonContainer;
		private ScintillaNet.Scintilla XmlInEditor;
		private ScintillaNet.Scintilla XmlOutEditor;
		private System.Windows.Forms.TextBox XmlInText;
		private System.Windows.Forms.Label XmlInLabel;
		private System.Windows.Forms.OpenFileDialog XmlInOpen;
		private System.Windows.Forms.Button ChooseXmlInButton;
		private System.Windows.Forms.Button RunButton;
		private System.Windows.Forms.Button LoadButton;
		private System.Windows.Forms.ComboBox TriggerCombo;
		private System.Windows.Forms.Label TriggerLabel;
	}
}

