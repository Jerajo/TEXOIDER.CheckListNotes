namespace CheckListNotes.PageModels.Commands
{
				using System;
				using System.Windows.Input;

				/// <summary>
				/// Clase ParamCommand: ICommand con parametro
				/// </summary>
				public class ParamCommand : ICommand
				{
								//atributos
								private readonly Action<object> _action;
								private readonly Predicate<object> _canExecute;
								private readonly Action RaiseCanExecuteChangedAction;
								public event EventHandler CanExecuteChanged;

								/// <summary>
								/// Constructor de ParamCommand(Action<object> action);
								/// </summary>
								/// <param name="action">Action a ser ejecutado.</param>
								public ParamCommand(Action<object> action) : this(action, null) { }

								/// <summary>
								/// Constructor de ParamCommand(Action<object> action, Func<bool> canExecute);
								/// </summary>
								/// <param name="action">Action a ser ejecutado.</param>
								/// <param name="canExecute">Funcion deternia si se puede ejecutar o no.</param>
								public ParamCommand(Action<object> action, Predicate<object> canExecute)
								{
								_action = action ?? throw new ArgumentNullException("execute");
								_canExecute = canExecute;
												this.RaiseCanExecuteChangedAction = RaiseCanExecuteChanged;
												SimpleCommandManager.AddRaiseCanExecuteChangedAction(ref RaiseCanExecuteChangedAction);
								}

								#region Métodos

								/// <summary>
								/// Método de la clase ParamCommand: Controla si se puede o no ejecutar un comando.
								/// </summary>
								/// <param name="parameter">Controlador del comando.</param>
								/// <returns>Retorna true o false segun el resultado del comando.</returns>
								public bool CanExecute(object parameter)
								{
								return _canExecute == null ? true : _canExecute(parameter);
								}

								/// <summary>
								/// Método de la clase ParamCommand: Comando a ser ejecutado.
								/// </summary>
								/// <param name="parameter">Parametro a enviado al comando.</param>
								public void Execute(object parameter)
								{
												_action(parameter);
												SimpleCommandManager.RefreshCommandStates();
								}

								/// <summary>
								/// Método de la ParamCommand: Manejada los eventos.
								/// </summary>
								public void RaiseCanExecuteChanged()
								{
												CanExecuteChanged?.Invoke(this, new EventArgs());
								}

								/// <summary>
								/// Método de la clase ParamCommand: Remueve la instancia del comando.
								/// </summary>
								public void RemoveCommand()
								{
												SimpleCommandManager.RemoveRaiseCanExecuteChangedAction(RaiseCanExecuteChangedAction);
								}

								#endregion

								/// <summary>
								/// Destructor de la clase ParamCommand.
								/// </summary>
								~ParamCommand()
								{
												RemoveCommand();
								}
				}
}
