using System.Drawing;
using System.Windows.Forms;

namespace MacroManager
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
            if (disposing && (components != null))
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
            this.components = new System.ComponentModel.Container();
            
            // Crear controles
            this.btnStartRecord = new System.Windows.Forms.Button();
            this.btnStopRecord = new System.Windows.Forms.Button();
            this.btnPlay = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnExport = new System.Windows.Forms.Button();
            this.btnImport = new System.Windows.Forms.Button();
            this.lstMacros = new System.Windows.Forms.ListBox();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblActionCount = new System.Windows.Forms.Label();
            this.groupBoxRecord = new System.Windows.Forms.GroupBox();
            this.groupBoxPlayback = new System.Windows.Forms.GroupBox();
            this.groupBoxManage = new System.Windows.Forms.GroupBox();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            
            this.groupBoxRecord.SuspendLayout();
            this.groupBoxPlayback.SuspendLayout();
            this.groupBoxManage.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            
            // 
            // groupBoxRecord
            // 
            this.groupBoxRecord.Controls.Add(this.btnStartRecord);
            this.groupBoxRecord.Controls.Add(this.btnStopRecord);
            this.groupBoxRecord.Controls.Add(this.lblActionCount);
            this.groupBoxRecord.Location = new System.Drawing.Point(12, 12);
            this.groupBoxRecord.Name = "groupBoxRecord";
            this.groupBoxRecord.Size = new System.Drawing.Size(260, 120);
            this.groupBoxRecord.TabIndex = 0;
            this.groupBoxRecord.TabStop = false;
            this.groupBoxRecord.Text = "Grabación";
            
            // 
            // btnStartRecord
            // 
            this.btnStartRecord.BackColor = System.Drawing.Color.FromArgb(76, 175, 80);
            this.btnStartRecord.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStartRecord.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnStartRecord.ForeColor = System.Drawing.Color.White;
            this.btnStartRecord.Location = new System.Drawing.Point(15, 30);
            this.btnStartRecord.Name = "btnStartRecord";
            this.btnStartRecord.Size = new System.Drawing.Size(110, 40);
            this.btnStartRecord.TabIndex = 0;
            this.btnStartRecord.Text = "⏺ Grabar";
            this.btnStartRecord.UseVisualStyleBackColor = false;
            this.btnStartRecord.Click += new System.EventHandler(this.btnStartRecord_Click);
            
            // 
            // btnStopRecord
            // 
            this.btnStopRecord.BackColor = System.Drawing.Color.FromArgb(244, 67, 54);
            this.btnStopRecord.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStopRecord.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnStopRecord.ForeColor = System.Drawing.Color.White;
            this.btnStopRecord.Location = new System.Drawing.Point(135, 30);
            this.btnStopRecord.Name = "btnStopRecord";
            this.btnStopRecord.Size = new System.Drawing.Size(110, 40);
            this.btnStopRecord.TabIndex = 1;
            this.btnStopRecord.Text = "⏹ Detener";
            this.btnStopRecord.UseVisualStyleBackColor = false;
            this.btnStopRecord.Click += new System.EventHandler(this.btnStopRecord_Click);
            
            // 
            // lblActionCount
            // 
            this.lblActionCount.AutoSize = true;
            this.lblActionCount.Location = new System.Drawing.Point(15, 85);
            this.lblActionCount.Name = "lblActionCount";
            this.lblActionCount.Size = new System.Drawing.Size(130, 15);
            this.lblActionCount.TabIndex = 2;
            this.lblActionCount.Text = "Acciones grabadas: 0";
            
            // 
            // groupBoxPlayback
            // 
            this.groupBoxPlayback.Controls.Add(this.btnPlay);
            this.groupBoxPlayback.Controls.Add(this.btnStop);
            this.groupBoxPlayback.Location = new System.Drawing.Point(12, 138);
            this.groupBoxPlayback.Name = "groupBoxPlayback";
            this.groupBoxPlayback.Size = new System.Drawing.Size(260, 90);
            this.groupBoxPlayback.TabIndex = 1;
            this.groupBoxPlayback.TabStop = false;
            this.groupBoxPlayback.Text = "Reproducción";
            
            // 
            // btnPlay
            // 
            this.btnPlay.BackColor = System.Drawing.Color.FromArgb(33, 150, 243);
            this.btnPlay.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPlay.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnPlay.ForeColor = System.Drawing.Color.White;
            this.btnPlay.Location = new System.Drawing.Point(15, 30);
            this.btnPlay.Name = "btnPlay";
            this.btnPlay.Size = new System.Drawing.Size(110, 40);
            this.btnPlay.TabIndex = 0;
            this.btnPlay.Text = "▶ Reproducir";
            this.btnPlay.UseVisualStyleBackColor = false;
            this.btnPlay.Click += new System.EventHandler(this.btnPlay_Click);
            
            // 
            // btnStop
            // 
            this.btnStop.BackColor = System.Drawing.Color.FromArgb(255, 152, 0);
            this.btnStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStop.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnStop.ForeColor = System.Drawing.Color.White;
            this.btnStop.Location = new System.Drawing.Point(135, 30);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(110, 40);
            this.btnStop.TabIndex = 1;
            this.btnStop.Text = "⏹ Parar";
            this.btnStop.UseVisualStyleBackColor = false;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            
            // 
            // groupBoxManage
            // 
            this.groupBoxManage.Controls.Add(this.btnSave);
            this.groupBoxManage.Controls.Add(this.btnDelete);
            this.groupBoxManage.Controls.Add(this.btnExport);
            this.groupBoxManage.Controls.Add(this.btnImport);
            this.groupBoxManage.Location = new System.Drawing.Point(12, 234);
            this.groupBoxManage.Name = "groupBoxManage";
            this.groupBoxManage.Size = new System.Drawing.Size(260, 150);
            this.groupBoxManage.TabIndex = 2;
            this.groupBoxManage.TabStop = false;
            this.groupBoxManage.Text = "Gestión";
            
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(15, 30);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(110, 35);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "💾 Guardar";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            
            // 
            // btnDelete
            // 
            this.btnDelete.Location = new System.Drawing.Point(135, 30);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(110, 35);
            this.btnDelete.TabIndex = 1;
            this.btnDelete.Text = "🗑 Eliminar";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(15, 75);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(110, 35);
            this.btnExport.TabIndex = 2;
            this.btnExport.Text = "📤 Exportar";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            
            // 
            // btnImport
            // 
            this.btnImport.Location = new System.Drawing.Point(135, 75);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(110, 35);
            this.btnImport.TabIndex = 3;
            this.btnImport.Text = "📥 Importar";
            this.btnImport.UseVisualStyleBackColor = true;
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            
            // 
            // lstMacros
            // 
            this.lstMacros.FormattingEnabled = true;
            this.lstMacros.ItemHeight = 15;
            this.lstMacros.Location = new System.Drawing.Point(290, 25);
            this.lstMacros.Name = "lstMacros";
            this.lstMacros.Size = new System.Drawing.Size(382, 359);
            this.lstMacros.TabIndex = 3;
            this.lstMacros.SelectedIndexChanged += new System.EventHandler(this.lstMacros_SelectedIndexChanged);
            
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblStatus.Location = new System.Drawing.Point(12, 400);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(80, 15);
            this.lblStatus.TabIndex = 4;
            this.lblStatus.Text = "⏸ Listo";
            
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 428);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(684, 22);
            this.statusStrip.TabIndex = 5;
            this.statusStrip.Text = "statusStrip1";
            
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(200, 17);
            this.toolStripStatusLabel.Text = "Macro Manager v1.0 - Listo";
            
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(684, 450);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.lstMacros);
            this.Controls.Add(this.groupBoxManage);
            this.Controls.Add(this.groupBoxPlayback);
            this.Controls.Add(this.groupBoxRecord);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Macro Manager para Videojuegos v1.0";
            this.groupBoxRecord.ResumeLayout(false);
            this.groupBoxRecord.PerformLayout();
            this.groupBoxPlayback.ResumeLayout(false);
            this.groupBoxManage.ResumeLayout(false);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button btnStartRecord;
        private System.Windows.Forms.Button btnStopRecord;
        private System.Windows.Forms.Button btnPlay;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.ListBox lstMacros;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblActionCount;
        private System.Windows.Forms.GroupBox groupBoxRecord;
        private System.Windows.Forms.GroupBox groupBoxPlayback;
        private System.Windows.Forms.GroupBox groupBoxManage;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
    }
}
