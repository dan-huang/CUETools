namespace JDP {
	partial class frmCUETools {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmCUETools));
			this.btnConvert = new System.Windows.Forms.Button();
			this.btnBrowseOutput = new System.Windows.Forms.Button();
			this.txtOutputPath = new System.Windows.Forms.TextBox();
			this.grpOutputStyle = new System.Windows.Forms.GroupBox();
			this.rbEmbedCUE = new System.Windows.Forms.RadioButton();
			this.rbGapsLeftOut = new System.Windows.Forms.RadioButton();
			this.rbGapsPrepended = new System.Windows.Forms.RadioButton();
			this.rbGapsAppended = new System.Windows.Forms.RadioButton();
			this.rbSingleFile = new System.Windows.Forms.RadioButton();
			this.btnAbout = new System.Windows.Forms.Button();
			this.grpOutputPathGeneration = new System.Windows.Forms.GroupBox();
			this.txtCustomFormat = new System.Windows.Forms.TextBox();
			this.rbCustomFormat = new System.Windows.Forms.RadioButton();
			this.txtCreateSubdirectory = new System.Windows.Forms.TextBox();
			this.rbDontGenerate = new System.Windows.Forms.RadioButton();
			this.rbCreateSubdirectory = new System.Windows.Forms.RadioButton();
			this.rbAppendFilename = new System.Windows.Forms.RadioButton();
			this.txtAppendFilename = new System.Windows.Forms.TextBox();
			this.grpAudioOutput = new System.Windows.Forms.GroupBox();
			this.btnCodec = new System.Windows.Forms.Button();
			this.rbUDC1 = new System.Windows.Forms.RadioButton();
			this.rbTTA = new System.Windows.Forms.RadioButton();
			this.chkLossyWAV = new System.Windows.Forms.CheckBox();
			this.rbAPE = new System.Windows.Forms.RadioButton();
			this.rbNoAudio = new System.Windows.Forms.RadioButton();
			this.rbWavPack = new System.Windows.Forms.RadioButton();
			this.rbWAV = new System.Windows.Forms.RadioButton();
			this.rbFLAC = new System.Windows.Forms.RadioButton();
			this.btnSettings = new System.Windows.Forms.Button();
			this.grpAction = new System.Windows.Forms.GroupBox();
			this.rbActionCorrectFilenames = new System.Windows.Forms.RadioButton();
			this.rbActionCreateCUESheet = new System.Windows.Forms.RadioButton();
			this.chkMulti = new System.Windows.Forms.CheckBox();
			this.rbActionVerifyAndCRCs = new System.Windows.Forms.RadioButton();
			this.rbActionVerifyAndEncode = new System.Windows.Forms.RadioButton();
			this.rbActionVerifyThenEncode = new System.Windows.Forms.RadioButton();
			this.rbActionVerify = new System.Windows.Forms.RadioButton();
			this.rbActionEncode = new System.Windows.Forms.RadioButton();
			this.txtPreGapLength = new System.Windows.Forms.MaskedTextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.txtDataTrackLength = new System.Windows.Forms.MaskedTextBox();
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripStatusLabelProcessed = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripStatusLabelWV = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripStatusLabelFLAC = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripStatusLabelAR = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
			this.toolStripProgressBar2 = new System.Windows.Forms.ToolStripProgressBar();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.btnStop = new System.Windows.Forms.Button();
			this.btnPause = new System.Windows.Forms.Button();
			this.btnResume = new System.Windows.Forms.Button();
			this.grpFreedb = new System.Windows.Forms.GroupBox();
			this.rbFreedbAlways = new System.Windows.Forms.RadioButton();
			this.rbFreedbIf = new System.Windows.Forms.RadioButton();
			this.rbFreedbNever = new System.Windows.Forms.RadioButton();
			this.contextMenuStripUDC = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
			this.tAKToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
			this.mP3ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.oGGToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.txtInputPath = new System.Windows.Forms.TextBox();
			this.grpInput = new System.Windows.Forms.GroupBox();
			this.textBatchReport = new System.Windows.Forms.TextBox();
			this.fileSystemTreeView1 = new CUEControls.FileSystemTreeView();
			this.grpExtra = new System.Windows.Forms.GroupBox();
			this.numericWriteOffset = new System.Windows.Forms.NumericUpDown();
			this.lblWriteOffset = new System.Windows.Forms.Label();
			this.contextMenuStripFileTree = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.SelectedNodeName = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.setAsMyMusicFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.resetToOriginalLocationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.panel1 = new System.Windows.Forms.Panel();
			this.grpOutputStyle.SuspendLayout();
			this.grpOutputPathGeneration.SuspendLayout();
			this.grpAudioOutput.SuspendLayout();
			this.grpAction.SuspendLayout();
			this.statusStrip1.SuspendLayout();
			this.grpFreedb.SuspendLayout();
			this.contextMenuStripUDC.SuspendLayout();
			this.grpInput.SuspendLayout();
			this.grpExtra.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericWriteOffset)).BeginInit();
			this.contextMenuStripFileTree.SuspendLayout();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnConvert
			// 
			resources.ApplyResources(this.btnConvert, "btnConvert");
			this.btnConvert.Name = "btnConvert";
			this.btnConvert.UseVisualStyleBackColor = true;
			this.btnConvert.Click += new System.EventHandler(this.btnConvert_Click);
			// 
			// btnBrowseOutput
			// 
			resources.ApplyResources(this.btnBrowseOutput, "btnBrowseOutput");
			this.btnBrowseOutput.Name = "btnBrowseOutput";
			this.btnBrowseOutput.UseVisualStyleBackColor = true;
			this.btnBrowseOutput.Click += new System.EventHandler(this.btnBrowseOutput_Click);
			// 
			// txtOutputPath
			// 
			this.txtOutputPath.AllowDrop = true;
			resources.ApplyResources(this.txtOutputPath, "txtOutputPath");
			this.txtOutputPath.Name = "txtOutputPath";
			this.txtOutputPath.DragDrop += new System.Windows.Forms.DragEventHandler(this.PathTextBox_DragDrop);
			this.txtOutputPath.DragEnter += new System.Windows.Forms.DragEventHandler(this.PathTextBox_DragEnter);
			// 
			// grpOutputStyle
			// 
			this.grpOutputStyle.Controls.Add(this.rbEmbedCUE);
			this.grpOutputStyle.Controls.Add(this.rbGapsLeftOut);
			this.grpOutputStyle.Controls.Add(this.rbGapsPrepended);
			this.grpOutputStyle.Controls.Add(this.rbGapsAppended);
			this.grpOutputStyle.Controls.Add(this.rbSingleFile);
			resources.ApplyResources(this.grpOutputStyle, "grpOutputStyle");
			this.grpOutputStyle.Name = "grpOutputStyle";
			this.grpOutputStyle.TabStop = false;
			// 
			// rbEmbedCUE
			// 
			resources.ApplyResources(this.rbEmbedCUE, "rbEmbedCUE");
			this.rbEmbedCUE.Name = "rbEmbedCUE";
			this.rbEmbedCUE.TabStop = true;
			this.toolTip1.SetToolTip(this.rbEmbedCUE, resources.GetString("rbEmbedCUE.ToolTip"));
			this.rbEmbedCUE.UseVisualStyleBackColor = true;
			this.rbEmbedCUE.CheckedChanged += new System.EventHandler(this.rbEmbedCUE_CheckedChanged);
			// 
			// rbGapsLeftOut
			// 
			resources.ApplyResources(this.rbGapsLeftOut, "rbGapsLeftOut");
			this.rbGapsLeftOut.Name = "rbGapsLeftOut";
			this.toolTip1.SetToolTip(this.rbGapsLeftOut, resources.GetString("rbGapsLeftOut.ToolTip"));
			this.rbGapsLeftOut.UseVisualStyleBackColor = true;
			// 
			// rbGapsPrepended
			// 
			resources.ApplyResources(this.rbGapsPrepended, "rbGapsPrepended");
			this.rbGapsPrepended.Name = "rbGapsPrepended";
			this.toolTip1.SetToolTip(this.rbGapsPrepended, resources.GetString("rbGapsPrepended.ToolTip"));
			this.rbGapsPrepended.UseVisualStyleBackColor = true;
			// 
			// rbGapsAppended
			// 
			resources.ApplyResources(this.rbGapsAppended, "rbGapsAppended");
			this.rbGapsAppended.Name = "rbGapsAppended";
			this.toolTip1.SetToolTip(this.rbGapsAppended, resources.GetString("rbGapsAppended.ToolTip"));
			this.rbGapsAppended.UseVisualStyleBackColor = true;
			// 
			// rbSingleFile
			// 
			resources.ApplyResources(this.rbSingleFile, "rbSingleFile");
			this.rbSingleFile.Checked = true;
			this.rbSingleFile.Name = "rbSingleFile";
			this.rbSingleFile.TabStop = true;
			this.toolTip1.SetToolTip(this.rbSingleFile, resources.GetString("rbSingleFile.ToolTip"));
			this.rbSingleFile.UseVisualStyleBackColor = true;
			// 
			// btnAbout
			// 
			resources.ApplyResources(this.btnAbout, "btnAbout");
			this.btnAbout.Name = "btnAbout";
			this.btnAbout.UseVisualStyleBackColor = true;
			this.btnAbout.Click += new System.EventHandler(this.btnAbout_Click);
			// 
			// grpOutputPathGeneration
			// 
			this.grpOutputPathGeneration.Controls.Add(this.btnBrowseOutput);
			this.grpOutputPathGeneration.Controls.Add(this.txtOutputPath);
			this.grpOutputPathGeneration.Controls.Add(this.txtCustomFormat);
			this.grpOutputPathGeneration.Controls.Add(this.rbCustomFormat);
			this.grpOutputPathGeneration.Controls.Add(this.txtCreateSubdirectory);
			this.grpOutputPathGeneration.Controls.Add(this.rbDontGenerate);
			this.grpOutputPathGeneration.Controls.Add(this.rbCreateSubdirectory);
			this.grpOutputPathGeneration.Controls.Add(this.rbAppendFilename);
			this.grpOutputPathGeneration.Controls.Add(this.txtAppendFilename);
			resources.ApplyResources(this.grpOutputPathGeneration, "grpOutputPathGeneration");
			this.grpOutputPathGeneration.Name = "grpOutputPathGeneration";
			this.grpOutputPathGeneration.TabStop = false;
			// 
			// txtCustomFormat
			// 
			resources.ApplyResources(this.txtCustomFormat, "txtCustomFormat");
			this.txtCustomFormat.Name = "txtCustomFormat";
			this.txtCustomFormat.TextChanged += new System.EventHandler(this.txtCustomFormat_TextChanged);
			// 
			// rbCustomFormat
			// 
			resources.ApplyResources(this.rbCustomFormat, "rbCustomFormat");
			this.rbCustomFormat.Name = "rbCustomFormat";
			this.rbCustomFormat.TabStop = true;
			this.rbCustomFormat.UseVisualStyleBackColor = true;
			this.rbCustomFormat.CheckedChanged += new System.EventHandler(this.rbCustomFormat_CheckedChanged);
			// 
			// txtCreateSubdirectory
			// 
			resources.ApplyResources(this.txtCreateSubdirectory, "txtCreateSubdirectory");
			this.txtCreateSubdirectory.Name = "txtCreateSubdirectory";
			this.txtCreateSubdirectory.TextChanged += new System.EventHandler(this.txtCreateSubdirectory_TextChanged);
			// 
			// rbDontGenerate
			// 
			resources.ApplyResources(this.rbDontGenerate, "rbDontGenerate");
			this.rbDontGenerate.Name = "rbDontGenerate";
			this.rbDontGenerate.UseVisualStyleBackColor = true;
			// 
			// rbCreateSubdirectory
			// 
			resources.ApplyResources(this.rbCreateSubdirectory, "rbCreateSubdirectory");
			this.rbCreateSubdirectory.Checked = true;
			this.rbCreateSubdirectory.Name = "rbCreateSubdirectory";
			this.rbCreateSubdirectory.TabStop = true;
			this.rbCreateSubdirectory.UseVisualStyleBackColor = true;
			this.rbCreateSubdirectory.CheckedChanged += new System.EventHandler(this.rbCreateSubdirectory_CheckedChanged);
			// 
			// rbAppendFilename
			// 
			resources.ApplyResources(this.rbAppendFilename, "rbAppendFilename");
			this.rbAppendFilename.Name = "rbAppendFilename";
			this.rbAppendFilename.UseVisualStyleBackColor = true;
			this.rbAppendFilename.CheckedChanged += new System.EventHandler(this.rbAppendFilename_CheckedChanged);
			// 
			// txtAppendFilename
			// 
			resources.ApplyResources(this.txtAppendFilename, "txtAppendFilename");
			this.txtAppendFilename.Name = "txtAppendFilename";
			this.txtAppendFilename.TextChanged += new System.EventHandler(this.txtAppendFilename_TextChanged);
			// 
			// grpAudioOutput
			// 
			this.grpAudioOutput.Controls.Add(this.btnCodec);
			this.grpAudioOutput.Controls.Add(this.rbUDC1);
			this.grpAudioOutput.Controls.Add(this.rbTTA);
			this.grpAudioOutput.Controls.Add(this.chkLossyWAV);
			this.grpAudioOutput.Controls.Add(this.rbAPE);
			this.grpAudioOutput.Controls.Add(this.rbNoAudio);
			this.grpAudioOutput.Controls.Add(this.rbWavPack);
			this.grpAudioOutput.Controls.Add(this.rbWAV);
			this.grpAudioOutput.Controls.Add(this.rbFLAC);
			resources.ApplyResources(this.grpAudioOutput, "grpAudioOutput");
			this.grpAudioOutput.Name = "grpAudioOutput";
			this.grpAudioOutput.TabStop = false;
			// 
			// btnCodec
			// 
			resources.ApplyResources(this.btnCodec, "btnCodec");
			this.btnCodec.Name = "btnCodec";
			this.btnCodec.UseVisualStyleBackColor = true;
			this.btnCodec.Click += new System.EventHandler(this.btnCodec_Click);
			// 
			// rbUDC1
			// 
			resources.ApplyResources(this.rbUDC1, "rbUDC1");
			this.rbUDC1.Name = "rbUDC1";
			this.rbUDC1.TabStop = true;
			this.rbUDC1.UseVisualStyleBackColor = true;
			this.rbUDC1.CheckedChanged += new System.EventHandler(this.rbUDC1_CheckedChanged);
			// 
			// rbTTA
			// 
			resources.ApplyResources(this.rbTTA, "rbTTA");
			this.rbTTA.Name = "rbTTA";
			this.rbTTA.TabStop = true;
			this.rbTTA.UseVisualStyleBackColor = true;
			this.rbTTA.CheckedChanged += new System.EventHandler(this.rbTTA_CheckedChanged);
			// 
			// chkLossyWAV
			// 
			resources.ApplyResources(this.chkLossyWAV, "chkLossyWAV");
			this.chkLossyWAV.Name = "chkLossyWAV";
			this.toolTip1.SetToolTip(this.chkLossyWAV, resources.GetString("chkLossyWAV.ToolTip"));
			this.chkLossyWAV.UseVisualStyleBackColor = true;
			this.chkLossyWAV.CheckedChanged += new System.EventHandler(this.chkLossyWAV_CheckedChanged);
			// 
			// rbAPE
			// 
			resources.ApplyResources(this.rbAPE, "rbAPE");
			this.rbAPE.Name = "rbAPE";
			this.rbAPE.TabStop = true;
			this.rbAPE.UseVisualStyleBackColor = true;
			this.rbAPE.CheckedChanged += new System.EventHandler(this.rbAPE_CheckedChanged);
			// 
			// rbNoAudio
			// 
			resources.ApplyResources(this.rbNoAudio, "rbNoAudio");
			this.rbNoAudio.Name = "rbNoAudio";
			this.toolTip1.SetToolTip(this.rbNoAudio, resources.GetString("rbNoAudio.ToolTip"));
			this.rbNoAudio.UseVisualStyleBackColor = true;
			this.rbNoAudio.CheckedChanged += new System.EventHandler(this.rbNoAudio_CheckedChanged);
			// 
			// rbWavPack
			// 
			resources.ApplyResources(this.rbWavPack, "rbWavPack");
			this.rbWavPack.Name = "rbWavPack";
			this.rbWavPack.UseVisualStyleBackColor = true;
			this.rbWavPack.CheckedChanged += new System.EventHandler(this.rbWavPack_CheckedChanged);
			// 
			// rbWAV
			// 
			resources.ApplyResources(this.rbWAV, "rbWAV");
			this.rbWAV.Checked = true;
			this.rbWAV.Name = "rbWAV";
			this.rbWAV.TabStop = true;
			this.rbWAV.UseVisualStyleBackColor = true;
			this.rbWAV.CheckedChanged += new System.EventHandler(this.rbWAV_CheckedChanged);
			// 
			// rbFLAC
			// 
			resources.ApplyResources(this.rbFLAC, "rbFLAC");
			this.rbFLAC.Name = "rbFLAC";
			this.rbFLAC.UseVisualStyleBackColor = true;
			this.rbFLAC.CheckedChanged += new System.EventHandler(this.rbFLAC_CheckedChanged);
			// 
			// btnSettings
			// 
			resources.ApplyResources(this.btnSettings, "btnSettings");
			this.btnSettings.Name = "btnSettings";
			this.btnSettings.UseVisualStyleBackColor = true;
			this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);
			// 
			// grpAction
			// 
			this.grpAction.Controls.Add(this.rbActionCorrectFilenames);
			this.grpAction.Controls.Add(this.rbActionCreateCUESheet);
			this.grpAction.Controls.Add(this.rbActionVerifyAndCRCs);
			this.grpAction.Controls.Add(this.rbActionVerifyAndEncode);
			this.grpAction.Controls.Add(this.rbActionVerifyThenEncode);
			this.grpAction.Controls.Add(this.rbActionVerify);
			this.grpAction.Controls.Add(this.rbActionEncode);
			resources.ApplyResources(this.grpAction, "grpAction");
			this.grpAction.Name = "grpAction";
			this.grpAction.TabStop = false;
			// 
			// rbActionCorrectFilenames
			// 
			resources.ApplyResources(this.rbActionCorrectFilenames, "rbActionCorrectFilenames");
			this.rbActionCorrectFilenames.Name = "rbActionCorrectFilenames";
			this.rbActionCorrectFilenames.TabStop = true;
			this.rbActionCorrectFilenames.UseVisualStyleBackColor = true;
			this.rbActionCorrectFilenames.CheckedChanged += new System.EventHandler(this.rbAction_CheckedChanged);
			// 
			// rbActionCreateCUESheet
			// 
			resources.ApplyResources(this.rbActionCreateCUESheet, "rbActionCreateCUESheet");
			this.rbActionCreateCUESheet.Name = "rbActionCreateCUESheet";
			this.rbActionCreateCUESheet.TabStop = true;
			this.rbActionCreateCUESheet.UseVisualStyleBackColor = true;
			this.rbActionCreateCUESheet.CheckedChanged += new System.EventHandler(this.rbAction_CheckedChanged);
			// 
			// chkMulti
			// 
			resources.ApplyResources(this.chkMulti, "chkMulti");
			this.chkMulti.Name = "chkMulti";
			this.chkMulti.UseVisualStyleBackColor = true;
			this.chkMulti.CheckedChanged += new System.EventHandler(this.chkMulti_CheckedChanged);
			// 
			// rbActionVerifyAndCRCs
			// 
			resources.ApplyResources(this.rbActionVerifyAndCRCs, "rbActionVerifyAndCRCs");
			this.rbActionVerifyAndCRCs.Name = "rbActionVerifyAndCRCs";
			this.toolTip1.SetToolTip(this.rbActionVerifyAndCRCs, resources.GetString("rbActionVerifyAndCRCs.ToolTip"));
			this.rbActionVerifyAndCRCs.UseVisualStyleBackColor = true;
			this.rbActionVerifyAndCRCs.CheckedChanged += new System.EventHandler(this.rbAction_CheckedChanged);
			// 
			// rbActionVerifyAndEncode
			// 
			resources.ApplyResources(this.rbActionVerifyAndEncode, "rbActionVerifyAndEncode");
			this.rbActionVerifyAndEncode.Name = "rbActionVerifyAndEncode";
			this.rbActionVerifyAndEncode.TabStop = true;
			this.rbActionVerifyAndEncode.UseVisualStyleBackColor = true;
			this.rbActionVerifyAndEncode.CheckedChanged += new System.EventHandler(this.rbAction_CheckedChanged);
			// 
			// rbActionVerifyThenEncode
			// 
			resources.ApplyResources(this.rbActionVerifyThenEncode, "rbActionVerifyThenEncode");
			this.rbActionVerifyThenEncode.Name = "rbActionVerifyThenEncode";
			this.toolTip1.SetToolTip(this.rbActionVerifyThenEncode, resources.GetString("rbActionVerifyThenEncode.ToolTip"));
			this.rbActionVerifyThenEncode.UseVisualStyleBackColor = true;
			this.rbActionVerifyThenEncode.CheckedChanged += new System.EventHandler(this.rbAction_CheckedChanged);
			// 
			// rbActionVerify
			// 
			resources.ApplyResources(this.rbActionVerify, "rbActionVerify");
			this.rbActionVerify.Name = "rbActionVerify";
			this.toolTip1.SetToolTip(this.rbActionVerify, resources.GetString("rbActionVerify.ToolTip"));
			this.rbActionVerify.UseVisualStyleBackColor = true;
			this.rbActionVerify.CheckedChanged += new System.EventHandler(this.rbAction_CheckedChanged);
			// 
			// rbActionEncode
			// 
			resources.ApplyResources(this.rbActionEncode, "rbActionEncode");
			this.rbActionEncode.Checked = true;
			this.rbActionEncode.Name = "rbActionEncode";
			this.rbActionEncode.TabStop = true;
			this.toolTip1.SetToolTip(this.rbActionEncode, resources.GetString("rbActionEncode.ToolTip"));
			this.rbActionEncode.UseVisualStyleBackColor = true;
			this.rbActionEncode.CheckedChanged += new System.EventHandler(this.rbAction_CheckedChanged);
			// 
			// txtPreGapLength
			// 
			this.txtPreGapLength.Culture = new System.Globalization.CultureInfo("");
			this.txtPreGapLength.CutCopyMaskFormat = System.Windows.Forms.MaskFormat.IncludePromptAndLiterals;
			this.txtPreGapLength.InsertKeyMode = System.Windows.Forms.InsertKeyMode.Overwrite;
			resources.ApplyResources(this.txtPreGapLength, "txtPreGapLength");
			this.txtPreGapLength.Name = "txtPreGapLength";
			this.txtPreGapLength.TextMaskFormat = System.Windows.Forms.MaskFormat.IncludePromptAndLiterals;
			this.toolTip1.SetToolTip(this.txtPreGapLength, resources.GetString("txtPreGapLength.ToolTip"));
			// 
			// label2
			// 
			resources.ApplyResources(this.label2, "label2");
			this.label2.Name = "label2";
			// 
			// label1
			// 
			resources.ApplyResources(this.label1, "label1");
			this.label1.Name = "label1";
			// 
			// txtDataTrackLength
			// 
			this.txtDataTrackLength.Culture = new System.Globalization.CultureInfo("");
			this.txtDataTrackLength.CutCopyMaskFormat = System.Windows.Forms.MaskFormat.IncludePromptAndLiterals;
			this.txtDataTrackLength.InsertKeyMode = System.Windows.Forms.InsertKeyMode.Overwrite;
			resources.ApplyResources(this.txtDataTrackLength, "txtDataTrackLength");
			this.txtDataTrackLength.Name = "txtDataTrackLength";
			this.txtDataTrackLength.TextMaskFormat = System.Windows.Forms.MaskFormat.IncludePromptAndLiterals;
			this.toolTip1.SetToolTip(this.txtDataTrackLength, resources.GetString("txtDataTrackLength.ToolTip"));
			// 
			// statusStrip1
			// 
			this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripStatusLabelProcessed,
            this.toolStripStatusLabel2,
            this.toolStripStatusLabelWV,
            this.toolStripStatusLabelFLAC,
            this.toolStripStatusLabelAR,
            this.toolStripProgressBar1,
            this.toolStripProgressBar2});
			resources.ApplyResources(this.statusStrip1, "statusStrip1");
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.ShowItemToolTips = true;
			this.statusStrip1.SizingGrip = false;
			// 
			// toolStripStatusLabel1
			// 
			this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
			resources.ApplyResources(this.toolStripStatusLabel1, "toolStripStatusLabel1");
			this.toolStripStatusLabel1.Spring = true;
			// 
			// toolStripStatusLabelProcessed
			// 
			this.toolStripStatusLabelProcessed.Name = "toolStripStatusLabelProcessed";
			resources.ApplyResources(this.toolStripStatusLabelProcessed, "toolStripStatusLabelProcessed");
			// 
			// toolStripStatusLabel2
			// 
			this.toolStripStatusLabel2.Image = global::JDP.Properties.Resources.wav;
			resources.ApplyResources(this.toolStripStatusLabel2, "toolStripStatusLabel2");
			this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
			this.toolStripStatusLabel2.Padding = new System.Windows.Forms.Padding(0, 0, 5, 0);
			// 
			// toolStripStatusLabelWV
			// 
			this.toolStripStatusLabelWV.Image = global::JDP.Properties.Resources.wv;
			resources.ApplyResources(this.toolStripStatusLabelWV, "toolStripStatusLabelWV");
			this.toolStripStatusLabelWV.Name = "toolStripStatusLabelWV";
			this.toolStripStatusLabelWV.Padding = new System.Windows.Forms.Padding(0, 0, 5, 0);
			// 
			// toolStripStatusLabelFLAC
			// 
			this.toolStripStatusLabelFLAC.Image = global::JDP.Properties.Resources.flac;
			this.toolStripStatusLabelFLAC.Name = "toolStripStatusLabelFLAC";
			this.toolStripStatusLabelFLAC.Padding = new System.Windows.Forms.Padding(0, 0, 5, 0);
			resources.ApplyResources(this.toolStripStatusLabelFLAC, "toolStripStatusLabelFLAC");
			// 
			// toolStripStatusLabelAR
			// 
			resources.ApplyResources(this.toolStripStatusLabelAR, "toolStripStatusLabelAR");
			this.toolStripStatusLabelAR.Name = "toolStripStatusLabelAR";
			this.toolStripStatusLabelAR.Padding = new System.Windows.Forms.Padding(0, 0, 5, 0);
			// 
			// toolStripProgressBar1
			// 
			this.toolStripProgressBar1.AutoToolTip = true;
			this.toolStripProgressBar1.Name = "toolStripProgressBar1";
			resources.ApplyResources(this.toolStripProgressBar1, "toolStripProgressBar1");
			this.toolStripProgressBar1.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
			// 
			// toolStripProgressBar2
			// 
			this.toolStripProgressBar2.AutoToolTip = true;
			this.toolStripProgressBar2.Name = "toolStripProgressBar2";
			resources.ApplyResources(this.toolStripProgressBar2, "toolStripProgressBar2");
			this.toolStripProgressBar2.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
			// 
			// toolTip1
			// 
			this.toolTip1.AutoPopDelay = 15000;
			this.toolTip1.InitialDelay = 500;
			this.toolTip1.ReshowDelay = 100;
			// 
			// btnStop
			// 
			resources.ApplyResources(this.btnStop, "btnStop");
			this.btnStop.Name = "btnStop";
			this.btnStop.UseVisualStyleBackColor = true;
			this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
			// 
			// btnPause
			// 
			resources.ApplyResources(this.btnPause, "btnPause");
			this.btnPause.Name = "btnPause";
			this.btnPause.UseVisualStyleBackColor = true;
			this.btnPause.Click += new System.EventHandler(this.btnPause_Click);
			// 
			// btnResume
			// 
			resources.ApplyResources(this.btnResume, "btnResume");
			this.btnResume.Name = "btnResume";
			this.btnResume.UseVisualStyleBackColor = true;
			this.btnResume.Click += new System.EventHandler(this.btnPause_Click);
			// 
			// grpFreedb
			// 
			this.grpFreedb.Controls.Add(this.rbFreedbAlways);
			this.grpFreedb.Controls.Add(this.rbFreedbIf);
			this.grpFreedb.Controls.Add(this.rbFreedbNever);
			resources.ApplyResources(this.grpFreedb, "grpFreedb");
			this.grpFreedb.Name = "grpFreedb";
			this.grpFreedb.TabStop = false;
			// 
			// rbFreedbAlways
			// 
			resources.ApplyResources(this.rbFreedbAlways, "rbFreedbAlways");
			this.rbFreedbAlways.Name = "rbFreedbAlways";
			this.rbFreedbAlways.TabStop = true;
			this.rbFreedbAlways.UseVisualStyleBackColor = true;
			// 
			// rbFreedbIf
			// 
			resources.ApplyResources(this.rbFreedbIf, "rbFreedbIf");
			this.rbFreedbIf.Name = "rbFreedbIf";
			this.rbFreedbIf.TabStop = true;
			this.rbFreedbIf.UseVisualStyleBackColor = true;
			// 
			// rbFreedbNever
			// 
			resources.ApplyResources(this.rbFreedbNever, "rbFreedbNever");
			this.rbFreedbNever.Name = "rbFreedbNever";
			this.rbFreedbNever.TabStop = true;
			this.rbFreedbNever.UseVisualStyleBackColor = true;
			// 
			// contextMenuStripUDC
			// 
			this.contextMenuStripUDC.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem2,
            this.tAKToolStripMenuItem,
            this.toolStripMenuItem1,
            this.toolStripSeparator1,
            this.toolStripMenuItem3,
            this.mP3ToolStripMenuItem,
            this.oGGToolStripMenuItem});
			this.contextMenuStripUDC.Name = "contextMenuStripUDC";
			resources.ApplyResources(this.contextMenuStripUDC, "contextMenuStripUDC");
			this.contextMenuStripUDC.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.contextMenuStripUDC_ItemClicked);
			// 
			// toolStripMenuItem2
			// 
			resources.ApplyResources(this.toolStripMenuItem2, "toolStripMenuItem2");
			this.toolStripMenuItem2.Name = "toolStripMenuItem2";
			// 
			// tAKToolStripMenuItem
			// 
			this.tAKToolStripMenuItem.Name = "tAKToolStripMenuItem";
			resources.ApplyResources(this.tAKToolStripMenuItem, "tAKToolStripMenuItem");
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			resources.ApplyResources(this.toolStripMenuItem1, "toolStripMenuItem1");
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
			// 
			// toolStripMenuItem3
			// 
			resources.ApplyResources(this.toolStripMenuItem3, "toolStripMenuItem3");
			this.toolStripMenuItem3.Name = "toolStripMenuItem3";
			// 
			// mP3ToolStripMenuItem
			// 
			this.mP3ToolStripMenuItem.Name = "mP3ToolStripMenuItem";
			resources.ApplyResources(this.mP3ToolStripMenuItem, "mP3ToolStripMenuItem");
			// 
			// oGGToolStripMenuItem
			// 
			this.oGGToolStripMenuItem.Name = "oGGToolStripMenuItem";
			resources.ApplyResources(this.oGGToolStripMenuItem, "oGGToolStripMenuItem");
			// 
			// txtInputPath
			// 
			this.txtInputPath.AllowDrop = true;
			resources.ApplyResources(this.txtInputPath, "txtInputPath");
			this.txtInputPath.BackColor = System.Drawing.SystemColors.Control;
			this.txtInputPath.Name = "txtInputPath";
			this.txtInputPath.TextChanged += new System.EventHandler(this.txtInputPath_TextChanged);
			this.txtInputPath.DragDrop += new System.Windows.Forms.DragEventHandler(this.PathTextBox_DragDrop);
			this.txtInputPath.DragEnter += new System.Windows.Forms.DragEventHandler(this.PathTextBox_DragEnter);
			// 
			// grpInput
			// 
			resources.ApplyResources(this.grpInput, "grpInput");
			this.grpInput.Controls.Add(this.textBatchReport);
			this.grpInput.Controls.Add(this.fileSystemTreeView1);
			this.grpInput.Controls.Add(this.txtInputPath);
			this.grpInput.Controls.Add(this.chkMulti);
			this.grpInput.Name = "grpInput";
			this.grpInput.TabStop = false;
			// 
			// textBatchReport
			// 
			resources.ApplyResources(this.textBatchReport, "textBatchReport");
			this.textBatchReport.Name = "textBatchReport";
			this.textBatchReport.ReadOnly = true;
			this.textBatchReport.TabStop = false;
			// 
			// fileSystemTreeView1
			// 
			this.fileSystemTreeView1.AllowDrop = true;
			resources.ApplyResources(this.fileSystemTreeView1, "fileSystemTreeView1");
			this.fileSystemTreeView1.BackColor = System.Drawing.SystemColors.Control;
			this.fileSystemTreeView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.fileSystemTreeView1.CheckBoxes = true;
			this.fileSystemTreeView1.FullRowSelect = true;
			this.fileSystemTreeView1.HideSelection = false;
			this.fileSystemTreeView1.ItemHeight = 16;
			this.fileSystemTreeView1.Name = "fileSystemTreeView1";
			this.fileSystemTreeView1.ShowLines = false;
			this.fileSystemTreeView1.ShowRootLines = false;
			this.fileSystemTreeView1.SpecialFolders = new CUEControls.ExtraSpecialFolder[] {
        CUEControls.ExtraSpecialFolder.MyComputer,
        CUEControls.ExtraSpecialFolder.Profile,
        CUEControls.ExtraSpecialFolder.MyMusic,
        CUEControls.ExtraSpecialFolder.CommonMusic};
			this.fileSystemTreeView1.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.fileSystemTreeView1_AfterCheck);
			this.fileSystemTreeView1.NodeExpand += new CUEControls.FileSystemTreeViewNodeExpandHandler(this.fileSystemTreeView1_NodeExpand);
			this.fileSystemTreeView1.DragDrop += new System.Windows.Forms.DragEventHandler(this.fileSystemTreeView1_DragDrop);
			this.fileSystemTreeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.fileSystemTreeView1_AfterSelect);
			this.fileSystemTreeView1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.fileSystemTreeView1_MouseDown);
			this.fileSystemTreeView1.DragEnter += new System.Windows.Forms.DragEventHandler(this.fileSystemTreeView1_DragEnter);
			this.fileSystemTreeView1.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.fileSystemTreeView1_AfterExpand);
			// 
			// grpExtra
			// 
			this.grpExtra.Controls.Add(this.numericWriteOffset);
			this.grpExtra.Controls.Add(this.txtPreGapLength);
			this.grpExtra.Controls.Add(this.lblWriteOffset);
			this.grpExtra.Controls.Add(this.label2);
			this.grpExtra.Controls.Add(this.txtDataTrackLength);
			this.grpExtra.Controls.Add(this.label1);
			resources.ApplyResources(this.grpExtra, "grpExtra");
			this.grpExtra.Name = "grpExtra";
			this.grpExtra.TabStop = false;
			// 
			// numericWriteOffset
			// 
			resources.ApplyResources(this.numericWriteOffset, "numericWriteOffset");
			this.numericWriteOffset.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.numericWriteOffset.Minimum = new decimal(new int[] {
            99999,
            0,
            0,
            -2147483648});
			this.numericWriteOffset.Name = "numericWriteOffset";
			// 
			// lblWriteOffset
			// 
			resources.ApplyResources(this.lblWriteOffset, "lblWriteOffset");
			this.lblWriteOffset.Name = "lblWriteOffset";
			// 
			// contextMenuStripFileTree
			// 
			this.contextMenuStripFileTree.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SelectedNodeName,
            this.toolStripSeparator2,
            this.setAsMyMusicFolderToolStripMenuItem,
            this.resetToOriginalLocationToolStripMenuItem});
			this.contextMenuStripFileTree.Name = "contextMenuStripFileTree";
			resources.ApplyResources(this.contextMenuStripFileTree, "contextMenuStripFileTree");
			// 
			// SelectedNodeName
			// 
			resources.ApplyResources(this.SelectedNodeName, "SelectedNodeName");
			this.SelectedNodeName.Name = "SelectedNodeName";
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
			// 
			// setAsMyMusicFolderToolStripMenuItem
			// 
			this.setAsMyMusicFolderToolStripMenuItem.Name = "setAsMyMusicFolderToolStripMenuItem";
			resources.ApplyResources(this.setAsMyMusicFolderToolStripMenuItem, "setAsMyMusicFolderToolStripMenuItem");
			this.setAsMyMusicFolderToolStripMenuItem.Click += new System.EventHandler(this.setAsMyMusicFolderToolStripMenuItem_Click);
			// 
			// resetToOriginalLocationToolStripMenuItem
			// 
			this.resetToOriginalLocationToolStripMenuItem.Name = "resetToOriginalLocationToolStripMenuItem";
			resources.ApplyResources(this.resetToOriginalLocationToolStripMenuItem, "resetToOriginalLocationToolStripMenuItem");
			this.resetToOriginalLocationToolStripMenuItem.Click += new System.EventHandler(this.resetToOriginalLocationToolStripMenuItem_Click);
			// 
			// panel1
			// 
			resources.ApplyResources(this.panel1, "panel1");
			this.panel1.Controls.Add(this.grpOutputPathGeneration);
			this.panel1.Controls.Add(this.btnStop);
			this.panel1.Controls.Add(this.btnConvert);
			this.panel1.Controls.Add(this.btnSettings);
			this.panel1.Controls.Add(this.grpExtra);
			this.panel1.Controls.Add(this.btnAbout);
			this.panel1.Controls.Add(this.grpOutputStyle);
			this.panel1.Controls.Add(this.grpFreedb);
			this.panel1.Controls.Add(this.grpAudioOutput);
			this.panel1.Controls.Add(this.grpAction);
			this.panel1.Controls.Add(this.btnPause);
			this.panel1.Controls.Add(this.btnResume);
			this.panel1.Name = "panel1";
			// 
			// frmCUETools
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.grpInput);
			this.Controls.Add(this.statusStrip1);
			this.MaximizeBox = false;
			this.Name = "frmCUETools";
			this.Load += new System.EventHandler(this.frmCUETools_Load);
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmCUETools_FormClosed);
			this.grpOutputStyle.ResumeLayout(false);
			this.grpOutputStyle.PerformLayout();
			this.grpOutputPathGeneration.ResumeLayout(false);
			this.grpOutputPathGeneration.PerformLayout();
			this.grpAudioOutput.ResumeLayout(false);
			this.grpAudioOutput.PerformLayout();
			this.grpAction.ResumeLayout(false);
			this.grpAction.PerformLayout();
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			this.grpFreedb.ResumeLayout(false);
			this.grpFreedb.PerformLayout();
			this.contextMenuStripUDC.ResumeLayout(false);
			this.grpInput.ResumeLayout(false);
			this.grpInput.PerformLayout();
			this.grpExtra.ResumeLayout(false);
			this.grpExtra.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericWriteOffset)).EndInit();
			this.contextMenuStripFileTree.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnConvert;
		private System.Windows.Forms.Button btnBrowseOutput;
		private System.Windows.Forms.TextBox txtOutputPath;
		private System.Windows.Forms.GroupBox grpOutputStyle;
		private System.Windows.Forms.Button btnAbout;
		private System.Windows.Forms.RadioButton rbGapsLeftOut;
		private System.Windows.Forms.RadioButton rbGapsPrepended;
		private System.Windows.Forms.RadioButton rbGapsAppended;
		private System.Windows.Forms.RadioButton rbSingleFile;
		private System.Windows.Forms.GroupBox grpOutputPathGeneration;
		private System.Windows.Forms.RadioButton rbDontGenerate;
		private System.Windows.Forms.RadioButton rbCreateSubdirectory;
		private System.Windows.Forms.RadioButton rbAppendFilename;
		private System.Windows.Forms.TextBox txtAppendFilename;
		private System.Windows.Forms.TextBox txtCreateSubdirectory;
		private System.Windows.Forms.GroupBox grpAudioOutput;
		private System.Windows.Forms.RadioButton rbFLAC;
		private System.Windows.Forms.RadioButton rbWAV;
		private System.Windows.Forms.RadioButton rbWavPack;
		private System.Windows.Forms.RadioButton rbCustomFormat;
		private System.Windows.Forms.TextBox txtCustomFormat;
		private System.Windows.Forms.Button btnSettings;
		private System.Windows.Forms.RadioButton rbNoAudio;
		private System.Windows.Forms.GroupBox grpAction;
		private System.Windows.Forms.RadioButton rbActionVerifyThenEncode;
		private System.Windows.Forms.RadioButton rbActionVerify;
		private System.Windows.Forms.RadioButton rbActionEncode;
		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
		private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
		private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar2;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.RadioButton rbEmbedCUE;
		private System.Windows.Forms.MaskedTextBox txtDataTrackLength;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.RadioButton rbAPE;
		private System.Windows.Forms.Button btnStop;
		private System.Windows.Forms.Button btnPause;
		private System.Windows.Forms.Button btnResume;
		private System.Windows.Forms.CheckBox chkLossyWAV;
		private System.Windows.Forms.RadioButton rbActionVerifyAndEncode;
		private System.Windows.Forms.RadioButton rbTTA;
		private System.Windows.Forms.GroupBox grpFreedb;
		private System.Windows.Forms.RadioButton rbFreedbAlways;
		private System.Windows.Forms.RadioButton rbFreedbIf;
		private System.Windows.Forms.RadioButton rbFreedbNever;
		private System.Windows.Forms.RadioButton rbUDC1;
		private System.Windows.Forms.Button btnCodec;
		private System.Windows.Forms.ContextMenuStrip contextMenuStripUDC;
		private System.Windows.Forms.ToolStripMenuItem tAKToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem mP3ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem oGGToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
		private System.Windows.Forms.RadioButton rbActionVerifyAndCRCs;
		private System.Windows.Forms.MaskedTextBox txtPreGapLength;
		private System.Windows.Forms.Label label2;
		private CUEControls.FileSystemTreeView fileSystemTreeView1;
		private System.Windows.Forms.TextBox txtInputPath;
		private System.Windows.Forms.CheckBox chkMulti;
		private System.Windows.Forms.GroupBox grpInput;
		private System.Windows.Forms.GroupBox grpExtra;
		private System.Windows.Forms.RadioButton rbActionCorrectFilenames;
		private System.Windows.Forms.RadioButton rbActionCreateCUESheet;
		private System.Windows.Forms.ContextMenuStrip contextMenuStripFileTree;
		private System.Windows.Forms.ToolStripMenuItem setAsMyMusicFolderToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem SelectedNodeName;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripMenuItem resetToOriginalLocationToolStripMenuItem;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelAR;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelFLAC;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelWV;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
		private System.Windows.Forms.NumericUpDown numericWriteOffset;
		private System.Windows.Forms.Label lblWriteOffset;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelProcessed;
		private System.Windows.Forms.TextBox textBatchReport;
		private System.Windows.Forms.Panel panel1;
	}
}

