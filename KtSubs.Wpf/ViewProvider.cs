using Autofac;
using KtSubs.Wpf.Exceptions;
using KtSubs.Wpf.Views;
using System;
using System.Reflection;
using System.Windows;

namespace KtSubs.Wpf
{
    public class ViewProvider
    {
        private readonly Assembly assemblyWithViewsAndViewModels = typeof(SelectionView).Assembly;
        private readonly ILifetimeScope lifetimeScope;

        public ViewProvider(ILifetimeScope lifetimeScope)
        {
            this.lifetimeScope = lifetimeScope;
        }

        public FrameworkElement GetView(Type viewModelType)
        {
            var viewModelFullName = viewModelType.FullName ?? throw new ViewNotFoundException("ViewModelType.Fullname");
            var viewFullName = viewModelFullName.Replace(".ViewModels.", ".Views.")[..^5];
            var viewType = assemblyWithViewsAndViewModels.GetType(viewFullName) ?? throw new ViewNotFoundException($"Class '${viewFullName}' not found");

            if (lifetimeScope.Resolve(viewType) is FrameworkElement view)
            {
                return view;
            }

            throw new ViewNotFoundException($"Class '${viewFullName}' is not a subtype of FrameworkElement");
        }
    }
}