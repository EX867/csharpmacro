using Macro;

class MacroTest{
    public void Run(MacroUtils.Data data){
        Console.WriteLine("[MacroTest] MacroTest start");
        MacroUtils utils = data.utils;
        StreamWriter writer_k = data.writer_keyboard;
        StreamWriter writer_m = data.writer_mouse;

        /*utils.SendMouseDown(writer_m, MouseCode.Left);
        utils.SendKeyDown(writer_k, KeyCode.KeyH);
        utils.SendKeyDown(writer_k, KeyCode.KeyE);
        utils.SendKeyDown(writer_k, KeyCode.KeyL);
        utils.SendKeyDown(writer_k, KeyCode.KeyL);
        utils.SendKeyDown(writer_k, KeyCode.KeyO);
        utils.SendKeyDown(writer_k, KeyCode.Key1);
        utils.SendKeyDown(writer_k, KeyMod.ShiftL, ()=>{
            utils.SendKeyDown(writer_k, KeyCode.Key1);
        });
        utils.SendKeyDown(writer_k, KeyMod.ShiftL, ()=>{
            utils.SendKeyDown(writer_k, KeyCode.Grave);
        });*/
        /*utils.SendKeyDown(writer_k, KeyMod.CtrlL, ()=>{
            utils.SendKeyDown(writer_k, KeyMod.ShiftL, ()=>{
                utils.SendKeyDown(writer_k, KeyCode.Key1);
            });
        });*/
        utils.SendMouseMoveAbsolute(writer_m, utils.ScreenWidth / 2, utils.ScreenHeight / 2);
        utils.WaitMs(1000); // 1 sec
        utils.SendMouseMoveAbsolute(writer_m, 50, 50);
        utils.WaitMs(1000); // 1 sec
        utils.SendMouseMoveAbsolute(writer_m, utils.ScreenWidth - 50, utils.ScreenHeight - 50);
        utils.WaitMs(1000); // 1 sec
        utils.SendMouseMoveAbsolute(writer_m, utils.ScreenWidth / 2, utils.ScreenHeight / 2);
        utils.WaitMs(1000); // 1 sec
        utils.SendMouseWheel(writer_m, 5);
        utils.WaitMs(1000); // 1 sec
        utils.SendMouseWheel(writer_m, -5);
    }
}









































