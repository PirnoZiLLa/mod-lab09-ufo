using System.Drawing;
using System.Windows.Forms;

namespace MotionSim
{
    partial class SimulationForm
    {
        private void SetupInterface()
        {
            _termsInput = new NumericUpDown
            {
                Location = new Point(20, 20),
                Size = new Size(120, 22),
                Name = "_termsInput",
                TabIndex = 0,
                Value = 2
            };

            _zoneSizeInput = new NumericUpDown
            {
                Location = new Point(20, 50),
                Size = new Size(120, 22),
                Name = "_zoneSizeInput",
                TabIndex = 1,
                Value = 2
            };

            _termsLabel = new System.Windows.Forms.Label
            {
                Location = new Point(150, 22),
                AutoSize = true,
                Name = "_termsLabel",
                TabIndex = 2,
                Text = "Члены ряда Тейлора"
            };

            _zoneLabel = new System.Windows.Forms.Label
            {
                Location = new Point(150, 52),
                AutoSize = true,
                Name = "_zoneLabel",
                TabIndex = 3,
                Text = "Радиус зоны"
            };

            _launchButton = new Button
            {
                Location = new Point(20, 80),
                Size = new Size(90, 28),
                Name = "_launchButton",
                TabIndex = 4,
                Text = "Старт",
                UseVisualStyleBackColor = true
            };
            _launchButton.Click += LaunchSimulation;

            _analyzeButton = new Button
            {
                Location = new Point(20, 115),
                Size = new Size(90, 28),
                Name = "_analyzeButton",
                TabIndex = 5,
                Text = "Анализ",
                UseVisualStyleBackColor = true
            };
            _analyzeButton.Click += PerformAnalysis;

            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(900, 600);
            Controls.AddRange(new Control[] { _termsInput, _zoneSizeInput, _termsLabel, _zoneLabel, _launchButton, _analyzeButton });
            Name = "SimulationForm";
            Text = "Симуляция движения";
            Paint += OnCanvasPaint;

            PerformLayout();
        }
    }
}