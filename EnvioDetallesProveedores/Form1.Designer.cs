namespace EnvioDetallesProveedores
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            flowLayoutPanel1 = new FlowLayoutPanel();
            txt_log = new RichTextBox();
            timerInicio = new System.Windows.Forms.Timer(components);
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Controls.Add(txt_log);
            flowLayoutPanel1.Location = new Point(-1, 0);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(419, 501);
            flowLayoutPanel1.TabIndex = 0;
            flowLayoutPanel1.WrapContents = false;
            // 
            // txt_log
            // 
            txt_log.BackColor = SystemColors.MenuText;
            txt_log.Font = new Font("Consolas", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            txt_log.ForeColor = Color.Lime;
            txt_log.Location = new Point(3, 3);
            txt_log.Name = "txt_log";
            txt_log.Size = new Size(406, 481);
            txt_log.TabIndex = 0;
            txt_log.Text = "";
            // 
            // timerInicio
            // 
            timerInicio.Enabled = true;
            timerInicio.Interval = 500;
            timerInicio.Tick += timerInicio_Tick;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(420, 496);
            Controls.Add(flowLayoutPanel1);
            Name = "Form1";
            Text = "Form1";
            flowLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Timer timerInicio;
        private RichTextBox txt_log;
    }
}
