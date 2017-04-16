namespace ExperimentMakerNew
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
            this.button1 = new System.Windows.Forms.Button();
            this.textBox_tempFolder = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_moreInfo = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label_CheckMinutes = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.numericUpDown_CheckMinutes = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.numericUpDown_numOfFiles = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label_ClientId = new System.Windows.Forms.Label();
            this.comboBox_clients = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label_client_description = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.dateTimePicker = new System.Windows.Forms.DateTimePicker();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_CheckMinutes)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_numOfFiles)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(18, 245);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(100, 23);
            this.button1.TabIndex = 12;
            this.button1.Text = "ייצר ניסוי";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // textBox_tempFolder
            // 
            this.textBox_tempFolder.Location = new System.Drawing.Point(108, 55);
            this.textBox_tempFolder.Name = "textBox_tempFolder";
            this.textBox_tempFolder.Size = new System.Drawing.Size(287, 20);
            this.textBox_tempFolder.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(416, 58);
            this.label1.Name = "label1";
            this.label1.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.label1.Size = new System.Drawing.Size(120, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "תיקיייה זמנית לניסוי:";
            // 
            // textBox_moreInfo
            // 
            this.textBox_moreInfo.Location = new System.Drawing.Point(108, 136);
            this.textBox_moreInfo.Name = "textBox_moreInfo";
            this.textBox_moreInfo.Size = new System.Drawing.Size(287, 20);
            this.textBox_moreInfo.TabIndex = 8;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(401, 139);
            this.label3.Name = "label3";
            this.label3.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.label3.Size = new System.Drawing.Size(135, 13);
            this.label3.TabIndex = 13;
            this.label3.Text = "הערות נוספות (באנגלית):";
            // 
            // label_CheckMinutes
            // 
            this.label_CheckMinutes.AutoSize = true;
            this.label_CheckMinutes.Location = new System.Drawing.Point(420, 96);
            this.label_CheckMinutes.Name = "label_CheckMinutes";
            this.label_CheckMinutes.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.label_CheckMinutes.Size = new System.Drawing.Size(116, 13);
            this.label_CheckMinutes.TabIndex = 14;
            this.label_CheckMinutes.Text = "כל כמה דקות לבדוק :";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 3.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(551, 1);
            this.label5.Name = "label5";
            this.label5.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.label5.Size = new System.Drawing.Size(14, 6);
            this.label5.TabIndex = 15;
            this.label5.Text = "בס\"ד";
            // 
            // numericUpDown_CheckMinutes
            // 
            this.numericUpDown_CheckMinutes.Location = new System.Drawing.Point(108, 94);
            this.numericUpDown_CheckMinutes.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDown_CheckMinutes.Name = "numericUpDown_CheckMinutes";
            this.numericUpDown_CheckMinutes.Size = new System.Drawing.Size(287, 20);
            this.numericUpDown_CheckMinutes.TabIndex = 7;
            this.numericUpDown_CheckMinutes.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(406, 174);
            this.label7.Name = "label7";
            this.label7.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.label7.Size = new System.Drawing.Size(130, 13);
            this.label7.TabIndex = 19;
            this.label7.Text = "כמות קלאסטרים לניסוי:";
            // 
            // numericUpDown_numOfFiles
            // 
            this.numericUpDown_numOfFiles.Location = new System.Drawing.Point(108, 172);
            this.numericUpDown_numOfFiles.Maximum = new decimal(new int[] {
            100000000,
            0,
            0,
            0});
            this.numericUpDown_numOfFiles.Name = "numericUpDown_numOfFiles";
            this.numericUpDown_numOfFiles.Size = new System.Drawing.Size(287, 20);
            this.numericUpDown_numOfFiles.TabIndex = 9;
            this.numericUpDown_numOfFiles.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.ForeColor = System.Drawing.Color.Red;
            this.label8.Location = new System.Drawing.Point(450, 246);
            this.label8.Name = "label8";
            this.label8.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.label8.Size = new System.Drawing.Size(86, 13);
            this.label8.TabIndex = 25;
            this.label8.Text = "מסומנים כחובה";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.ForeColor = System.Drawing.Color.Red;
            this.label12.Location = new System.Drawing.Point(542, 174);
            this.label12.Name = "label12";
            this.label12.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.label12.Size = new System.Drawing.Size(13, 16);
            this.label12.TabIndex = 26;
            this.label12.Text = "*";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.ForeColor = System.Drawing.Color.Red;
            this.label13.Location = new System.Drawing.Point(540, 243);
            this.label13.Name = "label13";
            this.label13.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.label13.Size = new System.Drawing.Size(13, 16);
            this.label13.TabIndex = 27;
            this.label13.Text = "*";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.ForeColor = System.Drawing.Color.Red;
            this.label14.Location = new System.Drawing.Point(542, 96);
            this.label14.Name = "label14";
            this.label14.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.label14.Size = new System.Drawing.Size(13, 16);
            this.label14.TabIndex = 28;
            this.label14.Text = "*";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label15.ForeColor = System.Drawing.Color.Red;
            this.label15.Location = new System.Drawing.Point(541, 55);
            this.label15.Name = "label15";
            this.label15.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.label15.Size = new System.Drawing.Size(13, 16);
            this.label15.TabIndex = 29;
            this.label15.Text = "*";
            // 
            // label_ClientId
            // 
            this.label_ClientId.AutoSize = true;
            this.label_ClientId.Location = new System.Drawing.Point(488, 20);
            this.label_ClientId.Name = "label_ClientId";
            this.label_ClientId.Size = new System.Drawing.Size(45, 13);
            this.label_ClientId.TabIndex = 36;
            this.label_ClientId.Text = ":ClientId";
            // 
            // comboBox_clients
            // 
            this.comboBox_clients.FormattingEnabled = true;
            this.comboBox_clients.Items.AddRange(new object[] {
            "Google Chrome",
            "Internet Explorer",
            "Mozilla Firefox"});
            this.comboBox_clients.Location = new System.Drawing.Point(225, 12);
            this.comboBox_clients.Name = "comboBox_clients";
            this.comboBox_clients.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.comboBox_clients.Size = new System.Drawing.Size(170, 21);
            this.comboBox_clients.TabIndex = 37;
            this.comboBox_clients.SelectedIndexChanged += new System.EventHandler(this.comboBox_clients_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Red;
            this.label2.Location = new System.Drawing.Point(541, 20);
            this.label2.Name = "label2";
            this.label2.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.label2.Size = new System.Drawing.Size(13, 16);
            this.label2.TabIndex = 38;
            this.label2.Text = "*";
            // 
            // label_client_description
            // 
            this.label_client_description.AutoSize = true;
            this.label_client_description.Location = new System.Drawing.Point(24, 15);
            this.label_client_description.Name = "label_client_description";
            this.label_client_description.Size = new System.Drawing.Size(0, 13);
            this.label_client_description.TabIndex = 39;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(420, 210);
            this.label4.Name = "label4";
            this.label4.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.label4.Size = new System.Drawing.Size(116, 13);
            this.label4.TabIndex = 40;
            this.label4.Text = "תאריך מחיקת הקובץ:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.Red;
            this.label6.Location = new System.Drawing.Point(542, 210);
            this.label6.Name = "label6";
            this.label6.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.label6.Size = new System.Drawing.Size(13, 16);
            this.label6.TabIndex = 41;
            this.label6.Text = "*";
            // 
            // dateTimePicker
            // 
            this.dateTimePicker.CustomFormat = "dd-MM-yyyy hh:mm:ss";
            this.dateTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePicker.Location = new System.Drawing.Point(108, 210);
            this.dateTimePicker.Name = "dateTimePicker";
            this.dateTimePicker.Size = new System.Drawing.Size(287, 20);
            this.dateTimePicker.TabIndex = 42;
            // 
            // MainForm
            // 
            this.AcceptButton = this.button1;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(565, 285);
            this.Controls.Add(this.dateTimePicker);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label_client_description);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.comboBox_clients);
            this.Controls.Add(this.label_ClientId);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.numericUpDown_numOfFiles);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.numericUpDown_CheckMinutes);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label_CheckMinutes);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox_moreInfo);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox_tempFolder);
            this.Controls.Add(this.button1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "יצירת ניסוי";
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_CheckMinutes)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_numOfFiles)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textBox_tempFolder;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_moreInfo;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label_CheckMinutes;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown numericUpDown_CheckMinutes;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown numericUpDown_numOfFiles;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label_ClientId;
        private System.Windows.Forms.ComboBox comboBox_clients;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label_client_description;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.DateTimePicker dateTimePicker;
    }
}

