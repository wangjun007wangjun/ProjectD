//配置
public class GameCfgConst
{
    //简单难度产生球的概率
    public static float[,] NumChance1 = new float[5, 2] { { 0, 0.8f }, { 0.8f, 1f }, { -1, -1 }, { -1, -1 }, { -1, -1 } }; 
    //一般难度产生球的概率
    public static float[,] NumChance2 = new float[5, 2] { { 0, 0.7f }, { 0.7f, 0.9f }, { 0.9f, 1f }, { -1, -1 }, { -1, -1 } }; 
    //困难难度产生球的概率
    public static float[,] NumChance3 = new float[5, 2] { { 0, 0.5f }, { 0.7f, 0.8f }, { 0.8f, 0.9f }, { 0.9f, 0.95f }, { 0.95f, 1f } }; 
    
    
    //简单难度产生球的类型概率
    public static float[,] TypeChance1 = new float[4, 2] { { 0, 0.9f }, { 0.9f, 0.95f }, { 0.95f, 1f }, { -1, -1 } };
    //一般难度产生球的类型概率
    public static float[,] TypeChance2 = new float[4, 2] { { 0, 0.7f }, { 0.7f, 0.8f }, { 0.8f, 0.9f }, { 0.9f, 1f } };
    //困难难度产生球的类型概率
    public static float[,] TypeChance3 = new float[4, 2] { { 0, 0.5f }, { 0.5f, 0.7f }, { 0.7f, 0.8f }, { 0.8f, 1f } };


    //简单难度产生多个球时的间隔
    public static float[] MulNumBornInterval1 = new float[2]{0.1f, 0.5f};
    //一般难度产生多个球时的间隔
    public static float[] MulNumBornInterval2 = new float[2]{0.1f, 0.3f};
    //困难难度产生多个球时的间隔
    public static float[] MulNumBornInterval3 = new float[2]{0.1f, 0.3f};
}
