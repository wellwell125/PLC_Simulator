using System.Transactions;
using System.Windows.Forms;

namespace PLC_Simulator
{
    public partial class Form1 : Form
    {
        PLC_Comm plc = new PLC_Comm();
        bool[] plc_in = new bool[16];
        bool[] plc_in_button = new bool[16];
        bool[] plc_out = new bool[16];
        int[] plc_data = new int[10];

        CYLINDER.cylinder_info[] arr_cylinder = new CYLINDER.cylinder_info[16];
        int movement_amount = 1;


        public enum direction
        {
            UP,
            RIGHT,
            DOWN,
            LEFT
        }

        string error_msg_plc = "通信が切断されました";
        //int edit_row = 0;


        public Form1()
        {
            InitializeComponent();

            for (int i = 0; i < 16; i++)
            {
                checkedListBox_plc_in_force.Items.Add("PLC_IN" + i);
                checkedListBox_plc_in.Items.Add(i);
                checkedListBox_plc_out.Items.Add(i);
                checkedListBox_plc_button.Items.Add(i);
            }

            comboBox_plc_maker.SelectedIndex = 0;


            checkedListBox_plc_in.Items[0] += " : " + "ワーク後退端";
            checkedListBox_plc_in.Items[1] += " : " + "ワーク前進端";
            checkedListBox_plc_in.Items[2] += " : " + "圧入上昇端";
            checkedListBox_plc_in.Items[3] += " : " + "圧入下降端";



            checkedListBox_plc_out.Items[0] += " : " + "ワーク前進";
            checkedListBox_plc_out.Items[1] += " : " + "ワーク後退";
            checkedListBox_plc_out.Items[2] += " : " + "圧入下降";
            checkedListBox_plc_out.Items[3] += " : " + "圧入上昇";

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CYLINDER cylinder_info = new CYLINDER();
            arr_cylinder = cylinder_info.set_info(this);

            set_plc_io();
        }

        private void set_plc_io()
        {
            if (comboBox_plc_maker.Text == "KEYENCE")
            {
                textBox_plc_in_type.Text = "R";
                textBox_plc_in_no.Text = "1000";

                textBox_plc_out_type.Text = "R";
                textBox_plc_out_no.Text = "5000";

                textBox_plc_in_button_type.Text = "MR";
                textBox_plc_in_button_no.Text = "1000";


                textBox_plc_data_type.Text = "DM";
                textBox_plc_data_no.Text = "0";
                int plc_data_no = int.Parse(textBox_plc_data_no.Text);

                dataGridView1.Rows.Clear();
                dataGridView1.Columns[1].Width = dataGridView1.Width - dataGridView1.Columns[0].Width - 5;
                for (int i = 0; i < 10; i++)
                {
                    dataGridView1.Rows.Add(textBox_plc_data_type.Text + (plc_data_no + i));
                }

            }
        }

        private void button_start_plc_comm_Click(object sender, EventArgs e)
        {

            if (button_start_plc_comm.BackColor == Color.Orange)
            {
                plc_disconnection();
                return;
            }

            plc.maker = comboBox_plc_maker.Text;
            string plc_ip = ip1.Text + "." + ip2.Text + "." + ip3.Text + "." + ip4.Text;
            int plc_port = int.Parse(port.Text);


            bool result = plc.connection(plc_ip, plc_port);
            if (result == false)
            {
                MessageBox.Show("PLCに接続できません");
                return;
            }

            timer_plc_comm.Enabled = true;
            button_start_plc_comm.BackColor = Color.Orange;
        }



        private void plc_disconnection()
        {
            button_start_plc_comm.BackColor = Color.White;
            timer_plc_comm.Enabled = false;
            plc.disconnection();

            for (int i = 0; i < plc_out.Length; i++)
            {
                plc_out[i] = false;
            }
        }



        private void timer_plc_comm_Tick(object sender, EventArgs e)
        {
            timer_plc_comm.Stop();

            plc_comm();

            timer_plc_comm.Start();
        }

        private void timer_update_form_Tick(object sender, EventArgs e)
        {
            timer_update_form.Stop();

            tabControl1.SuspendLayout();
            move_form();
            tabControl1.ResumeLayout();

            timer_update_form.Start();
        }


        private void plc_comm()
        {
            string io = "";
            bool result = false;


            for (int i = 0; i < 16; i++)
            {
                if (checkedListBox_plc_in_force.GetItemChecked(i))
                {
                    plc_in[i] = checkedListBox_plc_in.GetItemChecked(i);
                }

                plc_in_button[i] = checkedListBox_plc_button.GetItemChecked(i);
            }

            io = textBox_plc_in_type.Text + textBox_plc_in_no.Text;
            result = plc.write_plc_in(plc_in, io);
            if (result == false)
            {
                plc_disconnection();
                MessageBox.Show(error_msg_plc);
                return;
            }

            io = textBox_plc_in_button_type.Text + textBox_plc_in_button_no.Text;
            result = plc.write_plc_in(plc_in_button, io);
            if (result == false)
            {
                plc_disconnection();
                MessageBox.Show(error_msg_plc);
                return;
            }



            io = textBox_plc_out_type.Text + textBox_plc_out_no.Text;
            result = plc.read_plc_out(ref plc_out, io);
            if (result == false)
            {
                plc_disconnection();
                MessageBox.Show(error_msg_plc);
                return;
            }

            for (int i = 0; i < checkedListBox_plc_out.Items.Count; i++)
            {
                checkedListBox_plc_out.SetItemChecked(i, plc_out[i]);
            }




            io = textBox_plc_data_type.Text + textBox_plc_data_no.Text;
            result = plc.read_plc_data(ref plc_data, io);
            if (result == false)
            {
                plc_disconnection();
                MessageBox.Show(error_msg_plc);
                return;
            }

            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                dataGridView1[1, i].Value = plc_data[i].ToString();
            }
        }


        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if (dataGridView1[1, i].Value == null)
                {
                    dataGridView1[1, i].Value = 0;
                }

                plc_data[i] = int.Parse(dataGridView1[1, i].Value.ToString());
            }

            string io = textBox_plc_data_type.Text + textBox_plc_data_no.Text;
            bool result = plc.write_plc_data(ref plc_data, io);
            if (result == false)
            {
                plc_disconnection();
                MessageBox.Show(error_msg_plc);
                return;
            }
        }




        private void move_form()
        {
            this.SuspendLayout();

            for (int i = 0; i < plc_in.Length; i++)
            {
                TextBox ls = (TextBox)this.Controls["plc_in" + i];
                if (ls == null)
                {
                    continue;
                }

                if (plc_in[i])
                {
                    ls.BackColor = Color.Red;
                }
                else
                {
                    ls.BackColor = Color.White;
                }
            }

            move_cylinder();

            this.ResumeLayout();
        }


        private void move_cylinder()
        {
            for (int i = 0; i < arr_cylinder.Length; i++)
            {
                TextBox obj_cylinder = (TextBox)this.Controls["cylinder" + i];
                if (obj_cylinder == null)
                {
                    break;
                }

                move_sol(arr_cylinder[i]);
                int plc_out_move_no = arr_cylinder[i].plc_out_move_no;
                int plc_out_home_no = arr_cylinder[i].plc_out_home_no;

                if (plc_out_move_no == -1 && plc_out_home_no == -1)
                {
                    continue;
                }


                TextBox obj_group_main = (TextBox)this.Controls[arr_cylinder[i].parts_group[0]];

                //check cylinder ls

                switch (arr_cylinder[i].direction)
                {
                    case (int)direction.RIGHT:
                        break;

                    case (int)direction.LEFT:
                        if (obj_group_main.Left == arr_cylinder[i].move_pos_x)
                        {
                            plc_in[arr_cylinder[i].plc_in_move_no] = true;
                            plc_in[arr_cylinder[i].plc_in_home_no] = false;
                        }
                        else if (obj_group_main.Left == arr_cylinder[i].home_pos_x)
                        {
                            plc_in[arr_cylinder[i].plc_in_move_no] = false;
                            plc_in[arr_cylinder[i].plc_in_home_no] = true;
                        }
                        else
                        {
                            plc_in[arr_cylinder[i].plc_in_home_no] = false;
                            plc_in[arr_cylinder[i].plc_in_move_no] = false;
                        }

                        break;

                    case (int)direction.DOWN:
                        if (obj_group_main.Top == arr_cylinder[i].move_pos_y)
                        {
                            plc_in[arr_cylinder[i].plc_in_move_no] = true;
                            plc_in[arr_cylinder[i].plc_in_home_no] = false;
                        }
                        else if (obj_group_main.Top == arr_cylinder[i].home_pos_y)
                        {
                            plc_in[arr_cylinder[i].plc_in_move_no] = false;
                            plc_in[arr_cylinder[i].plc_in_home_no] = true;
                        }
                        else
                        {
                            plc_in[arr_cylinder[i].plc_in_move_no] = false;
                            plc_in[arr_cylinder[i].plc_in_home_no] = false;
                        }

                        break;

                    case (int)direction.UP:
                        break;
                }


                //check move amount
                bool is_move = false;
                int tmp_move_amount = 0;
                if (plc_out_move_no != -1)
                {
                    if (plc_out[plc_out_move_no])
                    {
                        is_move = true;
                        switch (arr_cylinder[i].direction)
                        {
                            case (int)direction.UP:
                            case (int)direction.LEFT:
                                tmp_move_amount = -movement_amount;
                                break;

                            case (int)direction.DOWN:
                            case (int)direction.RIGHT:
                                tmp_move_amount = movement_amount;
                                break;
                        }
                    }
                }
                if (plc_out_home_no != -1 && is_move == false)
                {
                    if (plc_out[plc_out_home_no])
                    {
                        switch (arr_cylinder[i].direction)
                        {
                            case (int)direction.UP:
                            case (int)direction.LEFT:
                                tmp_move_amount = movement_amount;
                                break;

                            case (int)direction.DOWN:
                            case (int)direction.RIGHT:
                                tmp_move_amount = -movement_amount;
                                break;
                        }
                    }
                }

                if (tmp_move_amount == 0 && arr_cylinder[i].is_release == false)
                {
                    continue;
                }
                else if (tmp_move_amount == 0 && arr_cylinder[i].is_release == true)
                {
                    switch (arr_cylinder[i].direction)
                    {
                        case (int)direction.UP:
                        case (int)direction.DOWN:
                            is_move = true;
                            tmp_move_amount = movement_amount;
                            break;
                    }
                }


                //check move
                switch (arr_cylinder[i].direction)
                {
                    case (int)direction.RIGHT:
                        break;

                    case (int)direction.LEFT:
                        if (is_move == true && obj_group_main.Left == arr_cylinder[i].move_pos_x)
                        {
                            continue;
                        }
                        else if (is_move == false && obj_group_main.Left == arr_cylinder[i].home_pos_x)
                        {
                            continue;
                        }
                        else if (obj_group_main.Left > arr_cylinder[i].home_pos_x)
                        {
                            tmp_move_amount = obj_group_main.Left - arr_cylinder[i].home_pos_x;
                            obj_group_main.Left = arr_cylinder[i].home_pos_x;
                        }

                        else if (obj_group_main.Left < arr_cylinder[i].move_pos_x)
                        {
                            tmp_move_amount = obj_group_main.Left - arr_cylinder[i].move_pos_x;
                            obj_group_main.Left = arr_cylinder[i].move_pos_x;
                        }
                        else
                        {
                            obj_group_main.Left += tmp_move_amount;
                        }

                        break;

                    case (int)direction.DOWN:
                        if ((is_move == true && obj_group_main.Top == arr_cylinder[i].move_pos_y))
                        {
                            continue;
                        }
                        else if (is_move == false && obj_group_main.Top == arr_cylinder[i].home_pos_y)
                        {
                            continue;
                        }
                        else if (obj_group_main.Top < arr_cylinder[i].home_pos_y)
                        {
                            tmp_move_amount = obj_group_main.Left - arr_cylinder[i].home_pos_y;
                            obj_group_main.Top = arr_cylinder[i].home_pos_y;
                        }

                        else if (obj_group_main.Top > arr_cylinder[i].move_pos_y)
                        {
                            tmp_move_amount = obj_group_main.Left - arr_cylinder[i].move_pos_y;
                            obj_group_main.Top = arr_cylinder[i].move_pos_y;
                        }
                        else
                        {
                            obj_group_main.Top += tmp_move_amount;
                        }

                        break;

                    case (int)direction.UP:
                        break;
                }


                for (int ii = 1; ii < arr_cylinder[i].parts_group.Count; ii++)
                {
                    TextBox obj_group = (TextBox)this.Controls[arr_cylinder[i].parts_group[ii]];
                    if (obj_group == null)
                    {
                        break;
                    }


                    switch (arr_cylinder[i].direction)
                    {
                        case (int)direction.UP:
                        case (int)direction.DOWN:
                            obj_group.Top += tmp_move_amount;
                            break;

                        case (int)direction.RIGHT:
                        case (int)direction.LEFT:
                            obj_group.Left += tmp_move_amount;
                            break;
                    }
                }

                
            }
        }


        private void move_sol(CYLINDER.cylinder_info cylinder)
        {
            Control cylincer_tab = this.tabControl1.Controls[0];

            int sol_no = cylinder.sol_no;
            int line_num = cylinder.line_num;
            int plc_out_home_no = cylinder.plc_out_home_no;
            int plc_out_move_no = cylinder.plc_out_move_no;

            if (sol_no == 0)
            {
                return;
            }

            Color color_left;
            Color color_right;

            if (plc_out[plc_out_move_no])
            {
                cylincer_tab.Controls["sol" + sol_no + "_3pos_double_A"].Visible = true;
                cylincer_tab.Controls["sol" + sol_no + "_3pos_double_B"].Visible = false;
                cylincer_tab.Controls["sol" + sol_no + "_3pos_double_C"].Visible = false;

                color_left = Color.Blue;
                color_right = Color.Red;
            }
            else if (plc_out[plc_out_home_no])
            {
                cylincer_tab.Controls["sol" + sol_no + "_3pos_double_A"].Visible = false;
                cylincer_tab.Controls["sol" + sol_no + "_3pos_double_B"].Visible = true;
                cylincer_tab.Controls["sol" + sol_no + "_3pos_double_C"].Visible = false;


                color_left = Color.Red;
                color_right = Color.Blue;
            }
            else
            {
                cylincer_tab.Controls["sol" + sol_no + "_3pos_double_A"].Visible = false;
                cylincer_tab.Controls["sol" + sol_no + "_3pos_double_B"].Visible = false;
                cylincer_tab.Controls["sol" + sol_no + "_3pos_double_C"].Visible = true;

                color_left = Color.White;
                color_right = Color.White;
            }

            for (int ii = 1; ii <= line_num; ii++)
            {
                cylincer_tab.Controls["sol" + sol_no + "_" + "line" + ii].BackColor = color_left;
            }
            for (int ii = 21; ii <= 20 + line_num; ii++)
            {
                cylincer_tab.Controls["sol" + sol_no + "_" + "line" + ii].BackColor = color_right;
            }

        }

        private void ip4_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\b')
            {
                return;
            }

            if ((e.KeyChar < '0' || '9' < e.KeyChar))
            {
                e.Handled = true;
            }
        }



    }
}