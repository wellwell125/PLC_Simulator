using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLC_Simulator
{
    internal class CYLINDER
    {
        public class cylinder_info
        {
            public int direction;
            public int home_pos_x;
            public int home_pos_y;
            public int move_pos_x;
            public int move_pos_y;

            public int plc_in_move_no;
            public int plc_in_home_no;
            public int plc_out_move_no;
            public int plc_out_home_no;

            public int sol_no;
            public int line_num;

            public bool is_release;

            public List<string> parts_group = new List<string>();
        }


        public cylinder_info[] set_info(Form1 form)
        {
            cylinder_info[] cylinder = new cylinder_info[16];

            TextBox obj_cylinder;
            for (int i = 0; i < cylinder.Length; i++)
            {
                cylinder[i] = new cylinder_info();
                obj_cylinder = (TextBox)form.Controls["cylinder" + i];
                if (obj_cylinder == null)
                {
                    continue;
                }

                cylinder[i].home_pos_x = obj_cylinder.Left;
                cylinder[i].home_pos_y = obj_cylinder.Top;

                cylinder[i].plc_in_move_no = -1;
                cylinder[i].plc_in_home_no = -1;
                cylinder[i].plc_out_move_no = -1;
                cylinder[i].plc_out_home_no = -1;
                cylinder[i].is_release = true;

                cylinder[i].parts_group.Add("cylinder" + i);
            }


            int cylinder_no;

            cylinder_no = 0;
            cylinder[cylinder_no].direction = (int)Form1.direction.LEFT;


            cylinder[cylinder_no].plc_in_move_no = 1;
            cylinder[cylinder_no].plc_in_home_no = cylinder[cylinder_no].plc_in_move_no - 1;

            cylinder[cylinder_no].plc_out_move_no = 0;
            cylinder[cylinder_no].plc_out_home_no = cylinder[cylinder_no].plc_out_move_no + 1;


            cylinder[cylinder_no].parts_group.Add("product_side");
            cylinder[cylinder_no].parts_group.Add("product_top");
            cylinder[cylinder_no].parts_group.Add("cylinder0_top");
            cylinder[cylinder_no].move_pos_x = form.Controls["base_side"].Left;
            cylinder[cylinder_no].sol_no = 1;
            cylinder[cylinder_no].line_num = 3;
            cylinder[cylinder_no].is_release = true;



            cylinder_no = 1;
            cylinder[cylinder_no].direction = (int)Form1.direction.DOWN;


            cylinder[cylinder_no].plc_in_move_no = 3;
            cylinder[cylinder_no].plc_in_home_no = cylinder[cylinder_no].plc_in_move_no - 1;

            cylinder[cylinder_no].plc_out_move_no = 2;
            cylinder[cylinder_no].plc_out_home_no = cylinder[cylinder_no].plc_out_move_no + 1;


            cylinder[cylinder_no].parts_group.Add("cylinder1_bar");
            cylinder[cylinder_no].move_pos_y = form.Controls["product_side"].Top - form.Controls["cylinder1"].Height;
            cylinder[cylinder_no].sol_no = 2;
            cylinder[cylinder_no].line_num = 4;
            cylinder[cylinder_no].is_release = true;

            return cylinder;
        }
    }
}
