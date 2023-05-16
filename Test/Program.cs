using PublicServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
namespace Test
{
    public class SetCommand
    {
        public ICommand ConcreteCommand { get; }
        public List<string> Arguments { get; }

        public SetCommand(string command, string arguments)
        {
            Arguments = arguments.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();

            // Получаем тип, соответствующий имени команды
            Type commandType = Type.GetType($"{GetType().Namespace}.Commands.{command}Command");

            // Создаем экземпляр типа и приводим его к интерфейсу ICommand
            ConcreteCommand = Activator.CreateInstance(commandType) as ICommand;

            //if (IsValideArguments())
            //{
            //    Execute();
            //}
        }


        //private bool IsValideArguments()
        //{
        //    if(ConcreteCommand.GetType().GetConstructors().Select(constructor => constructor.GetParameters().Length).Max() == Arguments.Count)
        //    {

        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        //private ICommand Execute()
        //{
            
        //}

    }


    internal class Program
    {
        static void Main(string[] args)
        {
            SetCommand command = new SetCommand("DeleteReciveCommand", "");
        }
    }
}
        
