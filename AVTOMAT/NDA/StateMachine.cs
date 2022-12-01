using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDA
{
    public class StateMachine
    {
        private List<string> _alphabet { get; set; }
        private List<string> _states { get; set; }
        private List<Transition> _transitions { get; set; }
        public string BeginState { get; set; }
        private List<string> _finalStates { get; set; }

        private string _input { get; set; }

        public StateMachine(string jsonFilePath)
        {
            using var reader = new StreamReader(jsonFilePath);
            var stateMachine = JsonConvert.DeserializeObject<StateMachineForConverting>(reader.ReadToEnd());
            //var nda = new StateMachine();
            //входные символы
            _alphabet = stateMachine.Alphabet;
            _states = stateMachine.States;
            BeginState = stateMachine.BeginState;
            //множество конечных состояний
            _finalStates = stateMachine.FinalStates;
            //переходы
            _transitions = stateMachine.Transitions;

            if (_alphabet.Any(a => a.Length > 1))
            {
                throw new Exception("Invalid automaton");
            }
            if (!_transitions.All(t => ValidateTransition(t)))
            {
                throw new Exception("Invalid automaton");
            }

            WriteStateMachine(stateMachine);

            NDAtoKDA(jsonFilePath);
        }

        public bool ValidateWord(List<string> currentStates, string input, List<string> steps)
        {
            Console.WriteLine(input);
            var transitions = new List<Transition>();
            _input = new string(input);

            for (int i = 0; i < input.Length; ++i)
            {
                var c = input[i];

                if (i == 0 && c == ' ')
                {
                    var transition = new Transition() { StartState = BeginState, EndState = BeginState, Symbol = " " };
                    Console.WriteLine(transition.ToString());
                }

                transitions = GetAllTransitions(currentStates, c.ToString()).ToList();

                transitions.AddRange(GetAllTransitions(transitions.Select(t => t.EndState).ToList(), " ").ToList());

                if (transitions.Any())
                {
                    foreach (var transition in transitions)
                    {
                        Console.WriteLine(transition.ToString());
                    }

                }
                else
                {
                    return false;
                }

                currentStates = transitions.Select(t => t.EndState).ToList();
            }
            if (_finalStates.Any(p => transitions.Any(t => t.EndState.Contains(p))))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void ValidateKDA()
        {
            var stateMachine = new KDA.StateMachine(@"C:\Users\Sergey\source\repos\AVTOMAT\NDA\testKDA.json");
            Console.WriteLine();
            Console.WriteLine("Проверка слова для полученного КДА");
            var result = stateMachine.ValidateWord(_input.ToCharArray());
            Console.WriteLine(result);
        }


        public void WriteStateMachine(StateMachineForConverting stateMachine)
        {
            string[,] statesResult = new string[stateMachine.States.Count + 1, stateMachine.Alphabet.Count + 1];

            for (int i = 1; i < statesResult.GetLength(1); i++)
            {
                if (stateMachine.Alphabet[i - 1] == " ")
                {
                    statesResult[0, i] = "e";
                }
                else
                {
                    statesResult[0, i] = stateMachine.Alphabet[i - 1];
                }
            }

            for (int i = 1; i < statesResult.GetLength(0); i++)
            {
                statesResult[i, 0] = stateMachine.States[i - 1];
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
                        if (stateMachine.Transitions.Any(t => t.StartState == statesResult[i, 0] && t.Symbol == stateMachine.Alphabet[j - 1]))
                        {
                            var ends = stateMachine.Transitions.Where(t => t.StartState == statesResult[i, 0] && t.Symbol == stateMachine.Alphabet[j - 1]).Select(y => y.EndState).ToList();

                            for (int k = 0; k < ends.Count; ++k)
                            {
                                if (stateMachine.FinalStates.Any(t => t == ends[k]))
                                {
                                    ends[k] = ends[k] + '*';
                                }
                            }

                            statesResult[i, j] = String.Join(", ", ends);
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
                if (stateMachine.FinalStates.Any(t => t == statesResult[i, 0]))
                {
                    statesResult[i, 0] = statesResult[i, 0] + '*';
                }
                if (stateMachine.BeginState == statesResult[i, 0])
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
                        s1 = statesResult[k, j].Length + 3;
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
                        if (statesResult[i, j].Any(t => t == ','))
                        {
                            Console.Write(statesResult[i, j] + new string(' ', sizes[j] - statesResult[i, j].Length - 2) + '\t');
                        }
                        else
                        {
                            Console.Write(statesResult[i, j] + new string(' ', sizes[j] - statesResult[i, j].Length - 1) + '\t');
                        }
                    }
                    else
                    {
                        if (statesResult[i, j].Any(t => t == ','))
                        {
                            Console.Write(statesResult[i, j] + new string(' ', sizes[j] - statesResult[i, j].Length - 2) + '\t');
                        }
                        else
                        {
                            if (i == 0 && j == 0)
                                Console.Write(statesResult[i, j] + '\t');
                            else
                                Console.Write(statesResult[i, j] + '\t');
                        }
                    }
                }
                Console.WriteLine();
            }
        }

        public void WriteStateMachineBezEps(StateMachineForConverting stateMachine)
        {
            string[,] statesResult = new string[stateMachine.States.Count + 1, stateMachine.Alphabet.Count];

            for (int i = 1; i < statesResult.GetLength(1); i++)
            {
                if (stateMachine.Alphabet[i - 1] == " ")
                {
                    statesResult[0, i] = "e";
                }
                else
                {
                    statesResult[0, i] = stateMachine.Alphabet[i - 1];
                }
            }

            for (int i = 1; i < statesResult.GetLength(0); i++)
            {
                statesResult[i, 0] = stateMachine.States[i - 1];
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
                        if (stateMachine.Transitions.Any(t => t.StartState == statesResult[i, 0] && t.Symbol == stateMachine.Alphabet[j - 1]))
                        {
                            var ends = stateMachine.Transitions.Where(t => t.StartState == statesResult[i, 0] && t.Symbol == stateMachine.Alphabet[j - 1]).Select(y => y.EndState).ToList();

                            for (int k = 0; k < ends.Count; ++k)
                            {
                                if (stateMachine.FinalStates.Any(t => t == ends[k]))
                                {
                                    ends[k] = ends[k] + '*';
                                }
                            }

                            statesResult[i, j] = String.Join(", ", ends);
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
                if (stateMachine.FinalStates.Any(t => t == statesResult[i, 0]))
                {
                    statesResult[i, 0] = statesResult[i, 0] + '*';
                }
                if (stateMachine.BeginState == statesResult[i, 0])
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
                        s1 = statesResult[k, j].Length + 3;
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
                        if (statesResult[i, j].Any(t => t == ','))
                        {
                            Console.Write(statesResult[i, j] + new string(' ', sizes[j] - statesResult[i, j].Length - 2) + '\t');
                        }
                        else
                        {
                            Console.Write(statesResult[i, j] + new string(' ', sizes[j] - statesResult[i, j].Length - 1) + '\t');
                        }
                    }
                    else
                    {
                        if (statesResult[i, j].Any(t => t == ','))
                        {
                            Console.Write(statesResult[i, j] + new string(' ', sizes[j] - statesResult[i, j].Length - 2) + '\t');
                        }
                        else
                        {
                            if (i == 0 && j == 0)
                                Console.Write(statesResult[i, j] + '\t');
                            else
                                Console.Write(statesResult[i, j] + '\t');
                        }
                    }
                }
                Console.WriteLine();
            }
        }

        public void NDAtoKDA(string jsonFilePath)
        {
            List<Transition> newTransitions = new List<Transition>();

            var startState = BeginState;

            using var reader = new StreamReader(jsonFilePath);
            var newStateMachine = JsonConvert.DeserializeObject<StateMachineForConverting>(reader.ReadToEnd());

            foreach (var transition in newStateMachine.Transitions)
            {
                if (transition.Symbol == " ")
                {
                    var newTransition = transition.StartState + $"+{transition.EndState}";

                    foreach (var state in newStateMachine.Transitions.Where(t => t.EndState == transition.StartState))
                    {
                        state.EndState = newTransition;
                    }
                    foreach (var state in newStateMachine.Transitions.Where(t => t.StartState == transition.EndState))
                    {
                        state.StartState = newTransition;
                    }
                    foreach (var state in newStateMachine.Transitions.Where(t => t.EndState == transition.EndState))
                    {
                        state.EndState = newTransition;
                    }
                    for (int i = 0; i < newStateMachine.States.Count; ++i)
                    {
                        if (newStateMachine.States[i] == transition.StartState || newStateMachine.States[i] == transition.EndState)
                        {
                            newStateMachine.States[i] = newTransition;
                        }
                    }
                    for (int i = 0; i < newStateMachine.FinalStates.Count; ++i)
                    {
                        if (newStateMachine.FinalStates[i] == transition.StartState || newStateMachine.FinalStates[i] == transition.EndState)
                        {
                            newStateMachine.FinalStates[i] = newTransition;
                        }
                    }
                    foreach (var state in newStateMachine.Transitions.Where(t => t.StartState == transition.StartState))
                    {
                        state.StartState = newTransition;
                    }
                }
            }

            newStateMachine.Transitions.RemoveAll(t => t.Symbol == " ");

            NewTransition(BeginState, ref newTransitions, newStateMachine);

            //Console.WriteLine("new state machine without e-ways");
            //foreach (var transition in newStateMachine.Transitions)
            //{
            //    Console.WriteLine($"{transition.StartState} -> {transition.EndState} ({transition.Symbol})");
            //}
            Console.WriteLine();
            Console.WriteLine("Таблица НКА без eps");
            WriteStateMachineBezEps(newStateMachine);
            Console.WriteLine();

            //Console.WriteLine("KDA ways");
            //foreach (var transition in newTransitions)
            //{
            //    Console.WriteLine($"{transition.StartState} -> {transition.EndState} ({transition.Symbol})");
            //}


            var stateMachineKDA = new KDA.StateMachineForConverting();
            stateMachineKDA.Alphabet = newStateMachine.Alphabet;
            stateMachineKDA.States = new List<KDA.State>();
            stateMachineKDA.Transitions = new List<KDA.Transition>();

            foreach (var tr in newTransitions)
            {
                stateMachineKDA.Transitions.Add(new KDA.Transition() { StartState = tr.StartState, EndState = tr.EndState, Condition = tr.Symbol });
            }

            foreach (var tr in newTransitions)
            {
                if (!stateMachineKDA.States.Any(s => s.Name == tr.StartState))
                {
                    stateMachineKDA.States.Add(new KDA.State() { Name = tr.StartState, IsEndState = newStateMachine.FinalStates.Any(t => t == tr.StartState) });
                }
                if (!stateMachineKDA.States.Any(s => s.Name == tr.EndState))
                {
                    stateMachineKDA.States.Add(new KDA.State() { Name = tr.EndState, IsEndState = newStateMachine.FinalStates.Any(t => t == tr.EndState) });
                }
            }

            using (var writer = new StreamWriter(@"C:\Users\Sergey\source\repos\AVTOMAT\NDA\testKDA.json"))
            {
                writer.Write(JsonConvert.SerializeObject(stateMachineKDA));
            }


        }

        private void NewTransition(string startStates, ref List<Transition> newTransitions, StateMachineForConverting stateMachine)
        {
            foreach (var startState in startStates.Split('+'))
            {
                if (stateMachine.Transitions.Any(t => t.StartState == startState))
                {
                    foreach (var alp in stateMachine.Alphabet)
                    {
                        if (alp != " ")
                        {
                            if (!newTransitions.Any(t => t.StartState == startState && t.Symbol == alp))
                            {
                                if (stateMachine.Transitions.Where(t => t.StartState == startState && t.Symbol == alp).Select(y => y.EndState).ToList().Count != 0)
                                {
                                    var endState = String.Join("-", stateMachine.Transitions.Where(t => t.StartState == startState && t.Symbol == alp).Select(y => y.EndState).ToList());
                                    newTransitions.Add(new Transition { StartState = startState, EndState = endState, Symbol = alp });

                                    foreach (var st in endState.Split('-'))
                                    {
                                        NewTransition(st, ref newTransitions, stateMachine);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private IEnumerable<Transition> GetAllTransitions(List<string> currentStates, string symbol)
        {
            return _transitions.FindAll(t => currentStates.Any(p => p.Contains(t.StartState)) && (t.Symbol == symbol));
        }

        private bool ValidateTransition(Transition transition)
        {
            return _states.Contains(transition.StartState) &&
                _states.Contains(transition.EndState) &&
                _alphabet.Contains(transition.Symbol);
        }

        public class StateMachineForConverting
        {
            public List<string> Alphabet { get; set; }
            public List<string> States { get; set; }
            public string BeginState { get; set; }
            public List<string> FinalStates { get; set; }
            public List<Transition> Transitions { get; set; }
        }
    }
}
