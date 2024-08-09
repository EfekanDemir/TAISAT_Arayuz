using System.Windows.Forms;

namespace TAISAT
{
    partial class Form2
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form2));
            this.form2_Save_Button = new System.Windows.Forms.Button();
            this.form2_Cancel_Button = new System.Windows.Forms.Button();
            this.form2_Label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.button_browseVideoFolderToSave = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.textBox_videoFolderToSave = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // form2_Save_Button
            // 
            this.form2_Save_Button.Location = new System.Drawing.Point(443, 214);
            this.form2_Save_Button.Name = "form2_Save_Button";
            this.form2_Save_Button.Size = new System.Drawing.Size(110, 40);
            this.form2_Save_Button.TabIndex = 1;
            this.form2_Save_Button.Text = "Kaydet";
            this.form2_Save_Button.UseVisualStyleBackColor = true;
            this.form2_Save_Button.Click += new System.EventHandler(this.form2_Save_Button_Click);
            // 
            // form2_Cancel_Button
            // 
            this.form2_Cancel_Button.Location = new System.Drawing.Point(559, 214);
            this.form2_Cancel_Button.Name = "form2_Cancel_Button";
            this.form2_Cancel_Button.Size = new System.Drawing.Size(110, 40);
            this.form2_Cancel_Button.TabIndex = 2;
            this.form2_Cancel_Button.Text = "İptal";
            this.form2_Cancel_Button.UseVisualStyleBackColor = true;
            this.form2_Cancel_Button.Click += new System.EventHandler(this.form2_Cancel_Button_Click);
            // 
            // form2_Label2
            // 
            this.form2_Label2.AutoSize = true;
            this.form2_Label2.BackColor = System.Drawing.Color.Transparent;
            this.form2_Label2.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.form2_Label2.ForeColor = System.Drawing.Color.Crimson;
            this.form2_Label2.Location = new System.Drawing.Point(14, 171);
            this.form2_Label2.Name = "form2_Label2";
            this.form2_Label2.Size = new System.Drawing.Size(0, 22);
            this.form2_Label2.TabIndex = 4;
            this.form2_Label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Font = new System.Drawing.Font("Times New Roman", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(522, 116);
            this.label1.TabIndex = 15;
            this.label1.Text = resources.GetString("label1.Text");
            // 
            // button_browseVideoFolderToSave
            // 
            this.button_browseVideoFolderToSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_browseVideoFolderToSave.BackgroundImage = global::TAISAT.Properties.Resources.menu;
            this.button_browseVideoFolderToSave.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.button_browseVideoFolderToSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.button_browseVideoFolderToSave.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.button_browseVideoFolderToSave.Location = new System.Drawing.Point(597, 28);
            this.button_browseVideoFolderToSave.Margin = new System.Windows.Forms.Padding(4);
            this.button_browseVideoFolderToSave.Name = "button_browseVideoFolderToSave";
            this.button_browseVideoFolderToSave.Size = new System.Drawing.Size(72, 22);
            this.button_browseVideoFolderToSave.TabIndex = 14;
            this.button_browseVideoFolderToSave.UseVisualStyleBackColor = true;
            this.button_browseVideoFolderToSave.Click += new System.EventHandler(this.button_browseVideoFolderToSave_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Image = global::TAISAT.Properties.Resources.info_5576171;
            this.pictureBox1.Location = new System.Drawing.Point(531, 3);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(130, 110);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 6;
            this.pictureBox1.TabStop = false;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 79.60848F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20.39152F));
            this.tableLayoutPanel1.Controls.Add(this.pictureBox1, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(5, 67);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(664, 116);
            this.tableLayoutPanel1.TabIndex = 16;
            // 
            // textBox_videoFolderToSave
            // 
            this.textBox_videoFolderToSave.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_videoFolderToSave.Location = new System.Drawing.Point(5, 28);
            this.textBox_videoFolderToSave.Name = "textBox_videoFolderToSave";
            this.textBox_videoFolderToSave.Size = new System.Drawing.Size(585, 22);
            this.textBox_videoFolderToSave.TabIndex = 17;
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(682, 266);
            this.Controls.Add(this.textBox_videoFolderToSave);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.button_browseVideoFolderToSave);
            this.Controls.Add(this.form2_Label2);
            this.Controls.Add(this.form2_Cancel_Button);
            this.Controls.Add(this.form2_Save_Button);
            this.ForeColor = System.Drawing.SystemColors.MenuText;
            this.Name = "Form2";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "TAISAT Message Box Video";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button form2_Save_Button;
        private System.Windows.Forms.Button form2_Cancel_Button;
        private System.Windows.Forms.Label form2_Label2;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button button_browseVideoFolderToSave;
        private Label label1;
        private TableLayoutPanel tableLayoutPanel1;
        private TextBox textBox_videoFolderToSave;
    }
}