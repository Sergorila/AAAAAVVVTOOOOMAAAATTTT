using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KDA
{
    public class StateMachine
    {
        private readonly State _initialState;
        private readonly List<State> _states;
        private readonly List<Transition> _transitions;
        private readonly List<string> _alphabet;

        public StateMachine(string jsonFilePath)
        {
            var stateMachine = CreateMachineByJson(jsonFilePath);
            _initialState = stateMachine.States[0];
            _states = stateMachine.States;
            _transitions = stateMachine.Transitions;
            _alphabet = stateMachine.Alphabet;

            if (!_transitions.All(t => ValidateTransition(t)))
            {
                throw new Exception("Invalid automaton");
            }

            WriteStateMachine(stateMachine);
        }

        public bool ValidateWord(char[] input)
        {
            var currentState = _initialState;

            for (int i = 0; i < input.Length; ++i)
            {
                var c = input[i];

                if (i == 0 && c == ' ')
                {
                    Console.WriteLine($"{c} => ({_states[0].Name}, {_states[0].Name})");
                }
                else
                {
                    var transition = GetNextTransition(currentState, c.ToString());
                    {
                        if (transition == null)
                            return false;
                    }

                    Console.WriteLine($"{c} => ({transition.StartState}, {transition.EndState})");

                    currentState = _states.FirstOrDefault(s => s.Name == transition.EndState);
                }
            }

            return IsEndState(currentState);
        }

        public void WriteStateMachine(StateMachineForConverting stateMachine)
        {
            string[,] statesResult = new string[stateMachine.States.Count + 1, stateMachine.Alphabet.Count + 1];

            for (int i = 1; i < statesResult.GetLength(1); i++)
            {
                statesResult[0, i] = stateMachine.Alphabet[i - 1];
            }

            for (int i = 1; i < statesResult.GetLength(0); i++)
            {
                statesResult[i, 0] = stateMachine.States[i - 1].Name;
            }

            for (int i = 0; i < statesResult.GetLength(0); i++)
            {
                for (int j = 0; j < statesResult.GetLength(1); j++)
                {
                    if (i == 0 && j == 0)
                    {
                        statesResult[i, j] = " ";
                    }
                    else if (i != 0 && j != 0)
                    {
                        if (stateMachine.Transitions.Any(t => t.StartState == statesResult[i, 0] && t.Condition == statesResult[0, j]))
                        {
                            statesResult[i, j] = stateMachine.Transitions.FirstOrDefault(t => t.StartState == statesResult[i, 0] && t.Condition == statesResult[0, j]).EndState;
                        }
                        else
                        {
                            statesResult[i, j] = "";
                        }
                    }
                }

            }

            for (int i = 1; i < statesResult.GetLength(0); i++)
            {
                if (stateMachine.Transitions.Any(t => t.EndState == statesResult[i, 0]))
                {
                    foreach (var item in stateMachine.States)
                    {
                        if (item.Name == statesResult[i, 0] && item.IsEndState)
                        {
                            statesResult[i, 0] = statesResult[i, 0] + '*';
                        }
                    }
                }
                if (stateMachine.States[0].Name == statesResult[i, 0])
                {
                    statesResult[i, 0] = "->" + statesResult[i, 0];
                }
            }

            List<int> sizes = new List<int>() { 0 };

            for (int j = 1; j < statesResult.GetLength(1); j++)
            {
                int s1 = 0;
                for (int k = 1; k < statesResult.GetLength(0); k++)
                {
                    if (statesResult[k, j].Length > s1)
                    {
                        s1 = statesResult[k, j].Length + 2;
                    }
                }
                sizes.Add(s1);
            }

            for (int i = 0; i < statesResult.GetLength(0); i++)
            {
                for (int j = 0; j < statesResult.GetLength(1); j++)
                {
                    if (statesResult[i, j].Length < sizes[j])
                    {

                        Console.Write(statesResult[i, j] + new string(' ', sizes[j] - statesResult[i, j].Length) + '\t');

                    }
                    else
                    {
                        Console.Write(statesResult[i, j] + '\t');

                    }
                }
                Console.WriteLine();
            }
        }
        private bool IsEndState(State state)
        {
            return _states.FirstOrDefault(s => s.Name == state.Name).IsEndState;
        }

        private Transition GetNextTransition(State startState, string c)
        {
            return _transitions.FirstOrDefault(t => t.StartState == startState.Name && t.Condition == c && _alphabet.Contains(c.ToString()));
        }

        private StateMachineForConverting CreateMachineByJson(string jsonFilePath)
        {
            using var reader = new StreamReader(jsonFilePath);
            return JsonConvert.DeserializeObject<StateMachineForConverting>(reader.ReadToEnd());
        }

        private bool ValidateTransition(Transition transition)
        {
            return _states.Any(s => transition.StartState.Contains(s.Name)) &&
                _states.Any(s => transition.EndState.Contains(s.Name));
        }

    }
    public class StateMachineForConverting
    {
        public List<string> Alphabet { get; set; }
        public List<State> States { get; set; }
        public List<Transition> Transitions { get; set; }
        public StateMachineForConverting() { }
    }
}
