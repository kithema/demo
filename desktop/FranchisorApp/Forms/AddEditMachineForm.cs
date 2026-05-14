using System;
using System.Drawing;
using System.Windows.Forms;
using FranchisorApp.Models;
using FranchisorApp.Services;

namespace FranchisorApp.Forms
{
    public partial class AddEditMachineForm : Form
    {
        private VendingMachine? _machine;
        private ApiService _apiService;
        private bool _isEdit;

        private TextBox txtSerial, txtInventory, txtLocation, txtModel, txtManufacturer;
        private DateTimePicker dtpManufacture, dtpCommissioning;
        private ComboBox cbStatus, cbCountry;
        private Button btnSave, btnCancel;

        public AddEditMachineForm(VendingMachine? machine = null)
        {
            InitializeComponent();
            _apiService = new ApiService();
            _machine = machine;
            _isEdit = machine != null;

            if (_isEdit)
            {
                this.Text = "Редактирование ТА";
                LoadMachineData();
            }
            else
            {
                this.Text = "Добавление ТА";
            }
        }

        private void InitializeComponent()
        {
            this.Size = new Size(500, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            int y = 20;
            int step = 35;

            AddField("Серийный номер:", ref txtSerial, ref y, step);
            AddField("Инвентарный номер:", ref txtInventory, ref y, step);
            AddField("Местоположение:", ref txtLocation, ref y, step);
            AddField("Модель:", ref txtModel, ref y, step);
            AddField("Производитель:", ref txtManufacturer, ref y, step);

            var lblManufacture = new Label { Text = "Дата изготовления:", Location = new Point(20, y), Size = new Size(120, 25) };
            dtpManufacture = new DateTimePicker { Location = new Point(150, y), Size = new Size(200, 25), Format = DateTimePickerFormat.Short };
            this.Controls.Add(lblManufacture);
            this.Controls.Add(dtpManufacture);
            y += step;

            var lblCommissioning = new Label { Text = "Дата ввода в эксплуатацию:", Location = new Point(20, y), Size = new Size(120, 25) };
            dtpCommissioning = new DateTimePicker { Location = new Point(150, y), Size = new Size(200, 25), Format = DateTimePickerFormat.Short };
            this.Controls.Add(lblCommissioning);
            this.Controls.Add(dtpCommissioning);
            y += step;

            var lblStatus = new Label { Text = "Статус:", Location = new Point(20, y), Size = new Size(120, 25) };
            cbStatus = new ComboBox { Location = new Point(150, y), Size = new Size(200, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cbStatus.Items.AddRange(new object[] { "Работает", "Вышел из строя", "В ремонте/на обслуживании" });
            cbStatus.SelectedIndex = 0;
            this.Controls.Add(lblStatus);
            this.Controls.Add(cbStatus);
            y += step;

            var lblCountry = new Label { Text = "Страна производства:", Location = new Point(20, y), Size = new Size(120, 25) };
            cbCountry = new ComboBox { Location = new Point(150, y), Size = new Size(200, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cbCountry.Items.AddRange(new object[] { "Россия", "Германия", "Китай", "США", "Япония" });
            cbCountry.SelectedIndex = 0;
            this.Controls.Add(lblCountry);
            this.Controls.Add(cbCountry);
            y += step + 20;

            btnSave = new Button { Text = "Сохранить", Location = new Point(100, y), Size = new Size(120, 35), BackColor = Color.FromArgb(40, 167, 69), ForeColor = Color.White };
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button { Text = "Отмена", Location = new Point(240, y), Size = new Size(120, 35), BackColor = Color.FromArgb(108, 117, 125), ForeColor = Color.White };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);
        }

        private void AddField(string labelText, ref TextBox textBox, ref int y, int step)
        {
            var label = new Label { Text = labelText, Location = new Point(20, y), Size = new Size(120, 25) };
            textBox = new TextBox { Location = new Point(150, y), Size = new Size(200, 25) };
            this.Controls.Add(label);
            this.Controls.Add(textBox);
            y += step;
        }

        private void LoadMachineData()
        {
            if (_machine == null) return;
            txtSerial.Text = _machine.serial_number;
            txtInventory.Text = _machine.inventory_number;
            txtLocation.Text = _machine.location;
            txtModel.Text = _machine.model;
            txtManufacturer.Text = _machine.manufacturer;
            dtpManufacture.Value = _machine.manufacture_date;
            dtpCommissioning.Value = _machine.commissioning_date;
            cbStatus.SelectedItem = _machine.status_name;
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSerial.Text))
            {
                MessageBox.Show("Введите серийный номер", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var machine = new VendingMachine
            {
                serial_number = txtSerial.Text,
                inventory_number = txtInventory.Text,
                location = txtLocation.Text,
                model = txtModel.Text,
                manufacturer = txtManufacturer.Text,
                manufacture_date = dtpManufacture.Value,
                commissioning_date = dtpCommissioning.Value,
                status_name = cbStatus.SelectedItem?.ToString(),
                country_name = cbCountry.SelectedItem?.ToString(),

                modem_id = $"MODEM_{new Random().Next(100, 999)}",
                company_name = "Новый франчайзи",
                current_cash = 0,
                connection_status = "Online",
                extra_status = "Норма"
            };

            try
            {
                if (_isEdit && _machine != null)
                {
                    machine.machine_id = _machine.machine_id;
                    await _apiService.UpdateVendingMachineAsync(_machine.machine_id, machine);
                }
                else
                {
                    await _apiService.CreateVendingMachineAsync(machine);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}