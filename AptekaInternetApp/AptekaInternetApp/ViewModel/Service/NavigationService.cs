using AptekaInternetApp.Models.ModelProgram;
using AptekaInternetApp.View.MainWindowFile.UsersControls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace AptekaInternetApp.ViewModel
{
    public static class NavigationService
    {
        private static Stack<NavigationItem> _backStack = new Stack<NavigationItem>();
        private static ContentControl _contentControl;
        private static object _currentCatalogState;

        // Событие для уведомления об изменении состояния навигации
        public static event Action NavigationStateChanged;

        public static void Initialize(ContentControl contentControl)
        {
            _contentControl = contentControl;
            Debug.WriteLine("NavigationService initialized");
        }

        public static void NavigateTo(UserControl userControl, object state = null)
        {
            Debug.WriteLine($"NavigateTo called: {userControl.GetType().Name}, State: {state != null}");

            // Сохраняем текущее состояние перед навигацией
            if (_contentControl.Content != null)
            {
                var currentState = GetCurrentState();
                _backStack.Push(new NavigationItem
                {
                    UserControl = _contentControl.Content,
                    State = currentState
                });
                Debug.WriteLine($"Pushed to back stack. Stack count: {_backStack.Count}");
            }

            _contentControl.Content = userControl;
            _currentCatalogState = state;

            // Восстанавливаем состояние если переходим в каталог
            if (userControl is CatalogUserControl catalog && state != null)
            {
                catalog.RestoreState(state);
            }

            // Уведомляем об изменении состояния навигации
            NavigationStateChanged?.Invoke();

            // Принудительно обновляем видимость кнопки назад
            UpdateBackButtonVisibility();
        }

        public static bool CanGoBack()
        {
            bool canGoBack = _backStack.Count > 0;
            Debug.WriteLine($"CanGoBack: {canGoBack} (Stack count: {_backStack.Count})");
            return canGoBack;
        }

        public static void GoBack()
        {
            Debug.WriteLine($"GoBack called. Stack count: {_backStack.Count}");

            if (_backStack.Count > 0)
            {
                var navigationItem = _backStack.Pop();
                _contentControl.Content = navigationItem.UserControl;
                _currentCatalogState = navigationItem.State;

                Debug.WriteLine($"Popped from stack. New stack count: {_backStack.Count}");

                // Восстанавливаем состояние каталога если нужно
                if (_contentControl.Content is CatalogUserControl catalog && navigationItem.State != null)
                {
                    catalog.RestoreState(navigationItem.State);

                    // ДОБАВЛЕНО: Обновляем состояние корзины после навигации
                    Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
                    {
                        catalog.RefreshCartState();
                    }), System.Windows.Threading.DispatcherPriority.Background);
                }

                // Уведомляем об изменении состояния навигации
                NavigationStateChanged?.Invoke();
                UpdateBackButtonVisibility();
            }
        }

        public static void ClearHistory()
        {
            _backStack.Clear();
            _currentCatalogState = null;
            NavigationStateChanged?.Invoke();
            UpdateBackButtonVisibility();
            Debug.WriteLine("Navigation history cleared");
        }

        private static object GetCurrentState()
        {
            if (_contentControl.Content is CatalogUserControl catalog)
            {
                return catalog.GetState();
            }
            else if (_contentControl.Content is ICustomStateRestorable restorable)
            {
                return restorable.GetState();
            }
            return _currentCatalogState;
        }

        private static void UpdateBackButtonVisibility()
        {
            // Находим MainWindow и обновляем кнопку назад
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                Debug.WriteLine($"Updating back button visibility. CanGoBack: {CanGoBack()}");
                mainWindow.UpdateBackButtonVisibility();
            }
            else
            {
                Debug.WriteLine("MainWindow not found for back button update");
            }
        }

        public static int GetStackCount()
        {
            return _backStack.Count;
        }
    }

    public class NavigationItem
    {
        public object UserControl { get; set; }
        public object State { get; set; }
    }

    // Интерфейс для контролов, которые поддерживают восстановление состояния
    public interface ICustomStateRestorable
    {
        object GetState();
        void RestoreState(object state);
    }
}