using System;
using System.Windows.Input;

namespace CheckListNotes.PageModels.Commands
{
				/// <summary>
				/// Clase DelegateCommand: Un comando que puede ser instanciado o eliminado segun sea necesario.
				/// </summary>
				public class DelegateCommand : ICommand
				{
								//atributos
								private readonly Action _action;
								private readonly Predicate<object> _canExecute;
								private readonly Action RaiseCanExecuteChangedAction;
								public event EventHandler CanExecuteChanged;

								/// <summary>
								/// Constructor de DelegateCommand(Action<object> action);
								/// </summary>
								/// <param name="action">Action a ser ejecutado.</param>
								public DelegateCommand(Action action) : this(action, null) { }

								/// <summary>
								/// Constructor de DelegateCommand(Action<object> action, Func<bool> canExecute);
								/// </summary>
								/// <param name="action">Action a ser ejecutado.</param>
								/// <param name="canExecute">Funcion deternia si se puede ejecutar o no.</param>
								public DelegateCommand(Action action, Predicate<object> canExecute)
								{
												_action = action ?? throw new ArgumentNullException("execute");
												_canExecute = canExecute;
												this.RaiseCanExecuteChangedAction = RaiseCanExecuteChanged;
												SimpleCommandManager.AddRaiseCanExecuteChangedAction(ref RaiseCanExecuteChangedAction);
								}

								#region Métodos

								/// <summary>
								/// Método de la clase DelegateCommand: Controla si se puede o no ejecutar un comando.
								/// </summary>
								/// <param name="parameter">Controlador del comando.</param>
								/// <returns>Retorna true o false segun el resultado del comando.</returns>
								public bool CanExecute(object parameter)
								{
												return _canExecute == null ? true : _canExecute(parameter);
								}

								/// <summary>
								/// Método de la clase DelegateCommand: Comando a ser ejecutado.
								/// </summary>
								/// <param name="parameter">Parametro a enviado al comando.</param>
								public void Execute(object parameter)
								{
												_action();
												SimpleCommandManager.RefreshCommandStates();
								}

								/// <summary>
								/// Método de la DelegateCommand: Manejada los eventos.
								/// </summary>
								public void RaiseCanExecuteChanged()
								{
												CanExecuteChanged?.Invoke(this, new EventArgs());
								}

								/// <summary>
								/// Método de la clase DelegateCommand: Remueve la instancia del comando.
								/// </summary>
								public void RemoveCommand()
								{
												SimpleCommandManager.RemoveRaiseCanExecuteChangedAction(RaiseCanExecuteChangedAction);
								}

								#endregion

								/// <summary>
								/// Destructor de la clase DelegateCommand.
								/// </summary>
								~DelegateCommand()
								{
												RemoveCommand();
								}
				}
}
