using System;
using System.Linq;
using MultiCraft.Scripts.Engine.Core.Worlds;
using MultiCraft.Scripts.Engine.Network;
using MultiCraft.Scripts.Engine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MultiCraft.Scripts.Engine.Utils.Commands
{
    public class CommandReader : MonoBehaviour
    {
        public TMP_InputField inputField;

        public RectTransform logsContainer;
        public TMP_Text logMessageTextPrefab;

        private InputSystem_Actions _inputSystem;

        private void Awake()
        {
            _inputSystem = new InputSystem_Actions();
            _inputSystem.Enable();

            _inputSystem.Player.Enable();
            _inputSystem.UI.Enable();
        }

        private void OnEnable()
        {
            _inputSystem.Player.SendMessage.performed += ReadCommand;
        }

        private void OnDisable()
        {
            _inputSystem.Player.SendMessage.performed -= ReadCommand;
        }

        private void OnDestroy()
        {
            _inputSystem.Player.Disable();
            _inputSystem.UI.Disable();
        }

        private void ReadCommand(InputAction.CallbackContext obj)
        {
            if (!UiManager.Instance.chatWindowOpen) return;


            var input = inputField.text;
            if (input[0] != '/')
            {
                
                if (NetworkManager.instance != null)
                {
                    PrintLog($"{NetworkManager.instance.playerName}: {input}");
                    NetworkManager.instance.SendMessageToServerChat(input);
                }
                else
                {
                    PrintLog($"{input}");
                }
                inputField.text = "";
                return;
            }

            var command = input.Substring(1);

            var commandParts = command.Split(' ');

            var commandName = commandParts[0];

            var commandParams = commandParts.Skip(1).ToArray();

            switch (commandName)
            {
                case "help":
                    PrintLog("Отображение справки");
                    break;
                case "say":
                    if (commandParams.Length > 0)
                    {
                        PrintLog("Сообщение: " + string.Join(" ", commandParams));
                    }
                    else
                    {
                        PrintLog("Не указано сообщение.");
                    }

                    break;
                case "struct":
                    if (commandParams.Length > 0)
                    {
                        switch (commandParams[0])
                        {
                            case "create":
                                if (commandParams.Length == 8)
                                {
                                    var structName = commandParams[1];
                                    var startPosition = new Vector3Int(
                                        int.Parse(commandParams[2]),
                                        int.Parse(commandParams[3]),
                                        int.Parse(commandParams[4])
                                    );
                                    var endPosition = new Vector3Int(
                                        int.Parse(commandParams[5]),
                                        int.Parse(commandParams[6]),
                                        int.Parse(commandParams[7])
                                    );
                                    var structure = World.Instance.CopyStructure(startPosition, endPosition);
                                    var path = World.Instance.SaveStructure(structure, structName);
                                    PrintLog("Новая структура сохранена: " + structName + " по пути: " + path);
                                }
                                else
                                {
                                    PrintLog("Неверное количество аргументов: " + commandName);
                                }

                                break;
                            case "place":
                                if (commandParams.Length == 5)
                                {
                                    var structName = commandParams[1];
                                    var placePosition = new Vector3Int(
                                        int.Parse(commandParams[2]),
                                        int.Parse(commandParams[3]),
                                        int.Parse(commandParams[4])
                                    );

                                    World.Instance.SpawnStructure(structName, placePosition);
                                    PrintLog("Структура заспавнена: " + structName + " " + placePosition);
                                }
                                else
                                {
                                    PrintLog("Неверное количество аргументов: " + commandName);
                                }

                                break;
                            default:
                                PrintLog("Неизвестная команда: " + commandName);
                                break;
                        }
                    }
                    else
                    {
                        PrintLog("Неверное количество аргументов: " + commandName);
                    }

                    break;
                default:
                    PrintLog("Неизвестная команда: " + commandName);
                    break;
            }

            inputField.text = "";
        }

        public void PrintLog(string log)
        {
            var message = Instantiate(logMessageTextPrefab, logsContainer);
            message.text = log;
        }
    }
}