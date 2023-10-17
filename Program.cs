using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Macro;

class Program
{
    static void Main(string[] args)
    {
        new MacroUtils(1920, 1080).Run((MacroUtils.Data data)=>{
            
            //if(data.eventValue == (int)EventValue.KeyPressed){
            //    if(data.eventCode == (int)KeyCode.KEY_1){
                    new MacroTest().Run(data); // thread
            //    }
            //}
        });

    }
}