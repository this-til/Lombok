namespace Til.Lombok;
//定义一个接口IFreeze，包含三个方法：
//1. isFrozen(string tag)：判断指定标签的元素是否被冻结
//2. frozen(string tag)：冻结指定标签的元素
//3. validateNonFrozen(string tag)：验证指定标签的元素是否没有被冻结
public interface IFreeze {
    /// <summary>
    /// 判断指定标签的元素是否被冻结
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    public bool isFrozen(string tag);
    /// <summary>
    /// 冻结指定标签的元素
    /// </summary>
    /// <param name="tag"></param>
    public void frozen(string tag);
    /// <summary>
    /// 验证指定标签的元素是否没有被冻结
    /// </summary>
    /// <param name="tag"></param>
    public void validateNonFrozen(string tag);
}