namespace WebServiceUtility
{
    partial class Encrypt
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
            this.txtStringtoEncrypt = new System.Windows.Forms.TextBox();
            this.lblStringtoEncrypt = new System.Windows.Forms.Label();
            this.lblEncryptedString = new System.Windows.Forms.Label();
            this.lblEncryptedKey = new System.Windows.Forms.Label();
            this.lblEncryptedIv = new System.Windows.Forms.Label();
            this.txtEncryptedString = new System.Windows.Forms.TextBox();
            this.txtEncryptedKey = new System.Windows.Forms.TextBox();
            this.txtEncryptedIv = new System.Windows.Forms.TextBox();
            this.btnEncrypt = new System.Windows.Forms.Button();
            this.btnReset = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtStringtoEncrypt
            // 
            this.txtStringtoEncrypt.Location = new System.Drawing.Point(132, 11);
            this.txtStringtoEncrypt.Multiline = true;
            this.txtStringtoEncrypt.Name = "txtStringtoEncrypt";
            this.txtStringtoEncrypt.Size = new System.Drawing.Size(437, 20);
            this.txtStringtoEncrypt.TabIndex = 0;
            // 
            // lblStringtoEncrypt
            // 
            this.lblStringtoEncrypt.AutoSize = true;
            this.lblStringtoEncrypt.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStringtoEncrypt.Location = new System.Drawing.Point(16, 14);
            this.lblStringtoEncrypt.Name = "lblStringtoEncrypt";
            this.lblStringtoEncrypt.Size = new System.Drawing.Size(110, 13);
            this.lblStringtoEncrypt.TabIndex = 1;
            this.lblStringtoEncrypt.Text = "String to Encrypt :";
            // 
            // lblEncryptedString
            // 
            this.lblEncryptedString.AutoSize = true;
            this.lblEncryptedString.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblEncryptedString.Location = new System.Drawing.Point(16, 53);
            this.lblEncryptedString.Name = "lblEncryptedString";
            this.lblEncryptedString.Size = new System.Drawing.Size(109, 13);
            this.lblEncryptedString.TabIndex = 2;
            this.lblEncryptedString.Text = "Encrypted String :";
            // 
            // lblEncryptedKey
            // 
            this.lblEncryptedKey.AutoSize = true;
            this.lblEncryptedKey.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblEncryptedKey.Location = new System.Drawing.Point(16, 92);
            this.lblEncryptedKey.Name = "lblEncryptedKey";
            this.lblEncryptedKey.Size = new System.Drawing.Size(97, 13);
            this.lblEncryptedKey.TabIndex = 3;
            this.lblEncryptedKey.Text = "Encrypted Key :";
            // 
            // lblEncryptedIv
            // 
            this.lblEncryptedIv.AutoSize = true;
            this.lblEncryptedIv.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblEncryptedIv.Location = new System.Drawing.Point(16, 131);
            this.lblEncryptedIv.Name = "lblEncryptedIv";
            this.lblEncryptedIv.Size = new System.Drawing.Size(87, 13);
            this.lblEncryptedIv.TabIndex = 4;
            this.lblEncryptedIv.Text = "Encrypted Iv :";
            // 
            // txtEncryptedString
            // 
            this.txtEncryptedString.Location = new System.Drawing.Point(132, 50);
            this.txtEncryptedString.Multiline = true;
            this.txtEncryptedString.Name = "txtEncryptedString";
            this.txtEncryptedString.ReadOnly = true;
            this.txtEncryptedString.Size = new System.Drawing.Size(437, 20);
            this.txtEncryptedString.TabIndex = 5;
            // 
            // txtEncryptedKey
            // 
            this.txtEncryptedKey.Location = new System.Drawing.Point(132, 89);
            this.txtEncryptedKey.Multiline = true;
            this.txtEncryptedKey.Name = "txtEncryptedKey";
            this.txtEncryptedKey.ReadOnly = true;
            this.txtEncryptedKey.Size = new System.Drawing.Size(437, 20);
            this.txtEncryptedKey.TabIndex = 6;
            // 
            // txtEncryptedIv
            // 
            this.txtEncryptedIv.Location = new System.Drawing.Point(132, 128);
            this.txtEncryptedIv.Multiline = true;
            this.txtEncryptedIv.Name = "txtEncryptedIv";
            this.txtEncryptedIv.ReadOnly = true;
            this.txtEncryptedIv.Size = new System.Drawing.Size(437, 20);
            this.txtEncryptedIv.TabIndex = 7;
            // 
            // btnEncrypt
            // 
            this.btnEncrypt.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnEncrypt.Location = new System.Drawing.Point(198, 169);
            this.btnEncrypt.Name = "btnEncrypt";
            this.btnEncrypt.Size = new System.Drawing.Size(83, 23);
            this.btnEncrypt.TabIndex = 8;
            this.btnEncrypt.Text = "Encrypt";
            this.btnEncrypt.UseVisualStyleBackColor = true;
            this.btnEncrypt.Click += new System.EventHandler(this.btnEncrypt_Click);
            // 
            // btnReset
            // 
            this.btnReset.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnReset.Location = new System.Drawing.Point(330, 169);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(75, 23);
            this.btnReset.TabIndex = 9;
            this.btnReset.Text = "Reset";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // Encrypt
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(582, 204);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.btnEncrypt);
            this.Controls.Add(this.txtEncryptedIv);
            this.Controls.Add(this.txtEncryptedKey);
            this.Controls.Add(this.txtEncryptedString);
            this.Controls.Add(this.lblEncryptedIv);
            this.Controls.Add(this.lblEncryptedKey);
            this.Controls.Add(this.lblEncryptedString);
            this.Controls.Add(this.lblStringtoEncrypt);
            this.Controls.Add(this.txtStringtoEncrypt);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(598, 243);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(598, 243);
            this.Name = "Encrypt";
            this.Text = "Encrypt";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtStringtoEncrypt;
        private System.Windows.Forms.Label lblStringtoEncrypt;
        private System.Windows.Forms.Label lblEncryptedString;
        private System.Windows.Forms.Label lblEncryptedKey;
        private System.Windows.Forms.Label lblEncryptedIv;
        private System.Windows.Forms.TextBox txtEncryptedString;
        private System.Windows.Forms.TextBox txtEncryptedKey;
        private System.Windows.Forms.TextBox txtEncryptedIv;
        private System.Windows.Forms.Button btnEncrypt;
        private System.Windows.Forms.Button btnReset;
    }
}

