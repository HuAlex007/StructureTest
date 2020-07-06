using System;
using System.Collections.Generic;

namespace ConsoleZuHe
{
    class Program
    {
        static void Main(string[] args)
        {
            //Create two nodes
            Node nodeWin = new Node("Window窗口");
            Node nodeFrame = new Node("Frame1");

            //Add items in Window窗口
            ITree picLeaf = new Leaf("Logo图片");
            ITree picReg = new Leaf("登录");
            ITree picLogin = new Leaf("注册");
            nodeWin.AddSub(picLeaf);
            nodeWin.AddSub(picReg);
            nodeWin.AddSub(picLogin);
            nodeWin.AddSub(nodeFrame);

            // add items in Frame1
            // ITree LabelUser = new Leaf("用户名");
            ITree TextBoxUser = new Leaf("用户名文本框");
            ITree LabelPW = new Leaf("密码");
            ITree TextBoxPW = new Leaf("密码文本框");
            ITree CheckBox = new Leaf("复选框");
            ITree LableRememberName = new Leaf("记住用户名");
            ITree LableForgotPW = new Leaf("忘记密码");
            nodeFrame.AddSub(new Leaf("用户名"));
            nodeFrame.AddSub(TextBoxUser);
            nodeFrame.AddSub(LabelPW);
            nodeFrame.AddSub(TextBoxPW);
            nodeFrame.AddSub(CheckBox);
            nodeFrame.AddSub(LableRememberName);
            nodeFrame.AddSub(LableForgotPW);

            //Print all items
            Console.WriteLine("******************开始打印******************");
            nodeWin.Do();
            Console.WriteLine("******************结束打印******************");
            Console.ReadKey();
        }
    }
    /// <summary>
    /// ITree
    /// </summary>
    public interface ITree
    {
        void Do();
    }
    /// <summary>
    /// Node
    /// </summary>
    public class Node : ITree
    {
        string nodeName;
        public List<ITree> subNodeList;

        public Node(string _name)
        {
            nodeName = _name;
            subNodeList = new List<ITree>();
        }

        public void Do()
        {
            Console.WriteLine("打印：" + nodeName);
            foreach (ITree item in subNodeList)
            {
                item.Do();
            }
        }

        public void AddSub(ITree subNode)
        {
            subNodeList.Add(subNode);
        }
    }
    /// <summary>
    /// Leaf
    /// </summary>
    public class Leaf : ITree
    {
        public string leafName;
        public Leaf(string _leafName)
        {
            leafName = _leafName;
        }
        public void Do()
        {
            Console.WriteLine("打印：" + leafName);
        }
    }
}
