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
            DG_MuestraComprobantes = new DataGridView();
            timerInicio = new System.Windows.Forms.Timer(components);
            flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)DG_MuestraComprobantes).BeginInit();
            SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Controls.Add(DG_MuestraComprobantes);
            flowLayoutPanel1.Location = new Point(-1, 0);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(419, 674);
            flowLayoutPanel1.TabIndex = 0;
            flowLayoutPanel1.WrapContents = false;
            // 
            // DG_MuestraComprobantes
            // 
            DG_MuestraComprobantes.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            DG_MuestraComprobantes.Location = new Point(3, 3);
            DG_MuestraComprobantes.Name = "DG_MuestraComprobantes";
            DG_MuestraComprobantes.Size = new Size(416, 489);
            DG_MuestraComprobantes.TabIndex = 1;
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
            ((System.ComponentModel.ISupportInitialize)DG_MuestraComprobantes).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private FlowLayoutPanel flowLayoutPanel1;
        private DataGridView DG_MuestraComprobantes;
        private System.Windows.Forms.Timer timerInicio;
    }
}
