using System.Linq;
using MultiCraft.Scripts.Engine.Core.Worlds;
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
        }

        private void OnEnable()
        {
            _inputSystem.Player.SendMessage.performed += ReadCommand;
        }

        private void OnDisable()
        {
            _inputSystem.Player.SendMessage.performed -= ReadCommand;
        }

        private void ReadCommand(InputAction.CallbackContext obj)
        {
            if (!UiManager.Instance.chatWindowOpen) return;

            var input = inputField.text;
            if (input[0] != '/')
            {
                inputField.text = "";
                return;
            }

            var command = input.Substring(1);

            var commandParts = command.Split(' ');

            var commandName = commandParts[0];

            var commandParams = commandParts.Skip(1).ToArray();

            var message = Instantiate(logMessageTextPrefab, logsContainer);

            switch (commandName)
            {
                case "help":
                    message.text = "Отображение справки";
                    ;
                    break;
                case "say":
                    if (commandParams.Length > 0)
                    {
                        message.text = "Сообщение: " + string.Join(" ", commandParams);
                    }
                    else
                    {
                        message.text = "Не указано сообщение.";
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
                                    var startPosition = new Vector3Int(int.Parse(commandParams[2]),
                                        int.Parse(commandParams[3]), int.Parse(commandParams[4]));
                                    var endPosition = new Vector3Int(int.Parse(commandParams[5]),
                                        int.Parse(commandParams[6]), int.Parse(commandParams[7]));
                                    var structure = World.Instance.CopyStructure(startPosition, endPosition);
                                    var path = World.Instance.SaveStructure(structure, structName);
                                    message.text = "Новая структура сохранена: " + structName + "по пути:" + path;
                                }
                                else
                                {
                                    message.text = "Неверное количество аргументов: " + commandName;
                                }

                                break;
                            case "place":
                                if (commandParams.Length == 5)
                                {
                                    var structName = commandParams[1];
                                    var placePosition = new Vector3Int(int.Parse(commandParams[2]),
                                        int.Parse(commandParams[3]), int.Parse(commandParams[4]));
                                    
                                    World.Instance.SpawnStructure(structName, placePosition);
            
                                    message.text = "Структура заспавнена: " + structName + " " + placePosition;
                                }
                                else
                                {
                                    message.text = "Неверное количество аргументов: " + commandName;
                                }

                                break;
                            default:
                                message.text = "Неизвестная команда: " + commandName;
                                break;
                        }
                    }
                    else
                    {
                        message.text = "Неверное количество аргументов: " + commandName;
                    }
                    break;
                default:
                    message.text = "Неизвестная команда: " + commandName;
                    break;
            }

            inputField.text = "";
        }
    }
}