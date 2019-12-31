namespace WebServiceUtility
{
    partial class ConvertFileToByte
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
            this.btnBrowse = new System.Windows.Forms.Button();
            this.lblSourcePath = new System.Windows.Forms.Label();
            this.txtSourcePath = new System.Windows.Forms.TextBox();
            this.btnReset = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnBrowse
            // 
            this.btnBrowse.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnBrowse.Location = new System.Drawing.Point(628, 17);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnBrowse.TabIndex = 0;
            this.btnBrowse.Text = "Browse";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // lblSourcePath
            // 
            this.lblSourcePath.AutoSize = true;
            this.lblSourcePath.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSourcePath.Location = new System.Drawing.Point(12, 22);
            this.lblSourcePath.Name = "lblSourcePath";
            this.lblSourcePath.Size = new System.Drawing.Size(85, 13);
            this.lblSourcePath.TabIndex = 1;
            this.lblSourcePath.Text = "Source Path :";
            // 
            // txtSourcePath
            // 
            this.txtSourcePath.Location = new System.Drawing.Point(103, 19);
            this.txtSourcePath.Name = "txtSourcePath";
            this.txtSourcePath.ReadOnly = true;
            this.txtSourcePath.Size = new System.Drawing.Size(519, 20);
            this.txtSourcePath.TabIndex = 2;
            // 
            // btnReset
            // 
            this.btnReset.Location = new System.Drawing.Point(289, 69);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(75, 23);
            this.btnReset.TabIndex = 3;
            this.btnReset.Text = "Reset";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // ConvertFileToByte
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(718, 132);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.txtSourcePath);
            this.Controls.Add(this.lblSourcePath);
            this.Controls.Add(this.btnBrowse);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConvertFileToByte";
            this.Text = "ConvertFileToByte";
            this.Load += new System.EventHandler(this.ConvertFileToByte_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Label lblSourcePath;
        private System.Windows.Forms.TextBox txtSourcePath;
        private System.Windows.Forms.Button btnReset;
    }
}

