namespace TatehamaTTC_v1bata.Window
{
    partial class MainWindow
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
            textBox1 = new TextBox();
            button1 = new Button();
            button2 = new Button();
            textBox2 = new TextBox();
            button3 = new Button();
            button4 = new Button();
            SuspendLayout();
            // 
            // textBox1
            // 
            textBox1.Location = new Point(12, 12);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(271, 23);
            textBox1.TabIndex = 0;
            // 
            // button1
            // 
            button1.Location = new Point(289, 12);
            button1.Name = "button1";
            button1.Size = new Size(62, 23);
            button1.TabIndex = 1;
            button1.Text = "扛上";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.Location = new Point(357, 12);
            button2.Name = "button2";
            button2.Size = new Size(62, 23);
            button2.TabIndex = 1;
            button2.Text = "落下";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // textBox2
            // 
            textBox2.Location = new Point(12, 58);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(86, 23);
            textBox2.TabIndex = 0;
            // 
            // button3
            // 
            button3.Location = new Point(104, 58);
            button3.Name = "button3";
            button3.Size = new Size(62, 23);
            button3.TabIndex = 1;
            button3.Text = "試験";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // button4
            // 
            button4.BackColor = Color.LightCoral;
            button4.Location = new Point(172, 58);
            button4.Name = "button4";
            button4.Size = new Size(62, 23);
            button4.TabIndex = 1;
            button4.Text = "全試験";
            button4.UseVisualStyleBackColor = false;
            button4.Click += button4_Click;
            // 
            // MainWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(472, 284);
            Controls.Add(button4);
            Controls.Add(button2);
            Controls.Add(button3);
            Controls.Add(button1);
            Controls.Add(textBox2);
            Controls.Add(textBox1);
            Name = "MainWindow";
            Text = "MainWindow";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox textBox1;
        private Button button1;
        private Button button2;
        private TextBox textBox2;
        private Button button3;
        private Button button4;
    }
}