using System;
using System.ComponentModel;
using System.Collections.Generic;

namespace CheckListNotes.PageModels.Commands
{
    /// <summary>
    /// Clase SimpleCommandManager: Maneja una lista de commandos estaticos.
    /// </summary>
    public static class SimpleCommandManager
    {
        //atributos
        private static List<Action> _raiseCanExecuteChangedActions = new List<Action>();

        /// <summary>
		      /// Método de la clase SimpleCommandManager: Añade un comando de control.
		      /// </summary>
        /// <param name="raiseCanExecuteChangedAction">Accion a ser ejecutada, al cambiar de accion.</param>
        public static void AddRaiseCanExecuteChangedAction(ref Action raiseCanExecuteChangedAction)
        {
            _raiseCanExecuteChangedActions.Add(raiseCanExecuteChangedAction);
        }

        /// <summary>
		      /// Método de la clase SimpleCommandManager: Elimina un comando de control.
		      /// </summary>
        /// <param name="raiseCanExecuteChangedAction">Accion a ser ejecutada, al cambiar de accion.</param>
        public static void RemoveRaiseCanExecuteChangedAction(Action raiseCanExecuteChangedAction)
        {
            _raiseCanExecuteChangedActions.Remove(raiseCanExecuteChangedAction);
        }

        /// <summary>
        /// Método de la clase SimpleCommandManager: Manejador de eventos asincronicos.
        /// </summary>
        /// <param name="propertyEventHandler">Manejador de eventos de propiedades.</param>
        public static void AssignOnPropertyChanged(ref PropertyChangedEventHandler propertyEventHandler)
        {
            propertyEventHandler += OnPropertyChanged;
        }

        /// <summary>
		      /// Método de la clase SimpleCommandManager: Evento que se ejecuta al canviar de propiedad.
		      /// </summary>
        /// <param name="sender">Objeto que disparo el evento.</param>
        /// <param name="e">Manejador del evento.</param>
        private static void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // this if clause is to prevent an infinity loop
            if (e.PropertyName != "CanExecute")
            {
                RefreshCommandStates();
            }
        }

        /// <summary>
		      /// Método de la clase SimpleCommandManager: Refresca los estados de los comandos.
		      /// </summary>
        public static void RefreshCommandStates()
        {
            for (var i = 0; i < _raiseCanExecuteChangedActions.Count; i++)
            {
                var raiseCanExecuteChangedAction = _raiseCanExecuteChangedActions[i];
                if (raiseCanExecuteChangedAction != null)
                {
                    raiseCanExecuteChangedAction.Invoke();
                }
            }
        }
    }
}
