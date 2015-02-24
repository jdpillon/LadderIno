﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Components.Logical
{
    /// <summary>
    /// Program rung 
    /// Represents one line in a ladder diagram.
    /// </summary>
    public class Rung : INotifyPropertyChanged
    {
        #region Properties
        /// <summary>
        /// Get or set (internally only) rung’s component collection
        /// </summary>
        public ObservableCollection<ComponentBase> Components
        {
            get { return _Components; }//set as internal
            private set
            {
                _Components = value;
                if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs("Components")); }                
            }
        }

        /// <summary>
        /// Get or set rung power
        /// </summary>
        public bool RungPower
        {
            get { return PowerRail.LogicLevel; }
            set
            {
                PowerRail.LogicLevel = value;
                if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs("RungPower")); }
            }
        }

        /// <summary>
        /// Get or set Comment in Rung
        /// </summary>
        public string Comment
        {
            get { return _Comment; }
            set
            {
                _Comment = value;
                if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs("Comment")); }
            }
        }

        #endregion Properties

        #region Functions

        /// <summary>
        /// Run logical test in the entire rung
        /// </summary>
        public void Execute()
        {
            if (RungPower)
            {
                foreach (ComponentBase comp in _Components) comp.Execute();
            }
        }


        #region Insert Functions
        //This sector contains all the functions that insert a component in the rung

        #region Auxiliar
        /// <summary>
        /// Check component nodes to avoid null references
        /// </summary>
        /// <param name="component">Component to be verified</param>
        private void CheckNodes(ComponentBase component)
        {
            if (component.LeftLide == null) component.LeftLide = new Node();
            if (component.RightLide == null) component.RightLide = new Node();
        }

        /// <summary>
        /// Verify component pair to be sure the insertion can happen 
        /// </summary>
        /// <param name="component">New component</param>
        /// <param name="anchor">Anchor component</param>
        private void CheckComponentPair(ComponentBase component, ComponentBase anchor)
        {
            if (anchor == null) throw new ArgumentException("Null component", "anchor");
            if (component == null) throw new ArgumentException("Null component", "component");
            if (!_Components.Contains(anchor)) throw new ArgumentException("Anchor component not inserted in current rung", "anchor");
            if (_Components.Contains(component)) throw new ArgumentException("Component already inserted in current rung", "component");
            if (component == anchor) throw new ArgumentException("Root components and inserter component are the same", "anchor");
            CheckNodes(component);
        }
        #endregion Auxiliar

        /// <summary>
        /// Inset component with auto select position 
        /// </summary>
        /// <param name="component">Component to be added</param>
        public void Add(ComponentBase component)
        {
            if (component == null) throw new ArgumentException("Null component","component");

            if (component.Class == ComponentBase.ComponentClass.Output)
            {
                if (_Components.Count == 0)
                {
                    ShortCircuit SC = new ShortCircuit(PowerRail);
                    SC.RightLide = new Node(SC);

                    component.RightLide = GroundRail;
                    component.LeftLide = SC.RightLide;

                    _Components.Add(SC);
                    _Components.Add(component);
                }
                else
                {
                    ComponentBase LC = _Components[_Components.Count - 1];

                    component.LeftLide = (LC.Class != ComponentBase.ComponentClass.Output) ? LC.RightLide : LC.LeftLide;
                    component.RightLide = GroundRail;

                    _Components.Add(component);
                }
            }
            else
            {
                if (_Components.Count == 0)
                {
                    component.LeftLide = PowerRail;
                    component.RightLide.Root = component;

                    _Components.Add(component);
                }
                else 
                {
                    ComponentBase FC = _Components[0];

                    component.LeftLide = PowerRail;

                    if (FC is ShortCircuit)
                    {
                        component.RightLide = FC.RightLide;
                        component.RightLide.Root = component;
                        _Components.Remove(FC);
                    }
                    else
                    {
                        component.RightLide.Root = component;
                        FC.LeftLide = component.RightLide;
                    }

                    _Components.Insert(0, component);
                }
            }
        }

        /// <summary>
        /// Insert above anchor component
        /// </summary>
        /// <param name="component">New component</param>
        /// <param name="anchor">Anchor component</param>
        public void InsertAbove(ComponentBase component, ComponentBase anchor)
        {
            CheckComponentPair(component, anchor);

            if (component.Class == ComponentBase.ComponentClass.Output)
            {
                if (anchor.Class != ComponentBase.ComponentClass.Output) throw new Exception("Can not insert output component above an input component");

                component.LeftLide = anchor.LeftLide;                
                component.RightLide = GroundRail;
                _Components.Insert(_Components.IndexOf(anchor), component);
            }
            else
            {
                if (anchor.Class == ComponentBase.ComponentClass.Output) throw new Exception("Can not insert input component above an output component");

                if (anchor.RightLide.Root == anchor) anchor.RightLide.Root = component;

                component.LeftLide = anchor.LeftLide;
                component.RightLide = anchor.RightLide;
                _Components.Insert(_Components.IndexOf(anchor), component);

                if (anchor is ShortCircuit) _Components.Remove(anchor);
            }
        }

        /// <summary>
        /// Insert under anchor component
        /// </summary>
        /// <param name="component">New component</param>
        /// <param name="anchor">Anchor component</param>
        public void InsertUnder(ComponentBase component, ComponentBase anchor)
        {
            CheckComponentPair(component, anchor);

            if (component.Class == ComponentBase.ComponentClass.Output)
            {
                if (anchor.Class != ComponentBase.ComponentClass.Output) throw new Exception("Can not insert output component under an input component");

                component.LeftLide = anchor.LeftLide;
                component.RightLide = GroundRail;
                _Components.Add(component);
            }
            else
            {
                if (anchor.Class == ComponentBase.ComponentClass.Output) throw new Exception("Can not insert input component under an output component");

                component.LeftLide = anchor.LeftLide;
                component.RightLide = anchor.RightLide;
                _Components.Insert(_Components.IndexOf(anchor) + 1, component);

                if (anchor is ShortCircuit)
                {
                    component.RightLide.Root = component;
                    _Components.Remove(anchor);
                }
            }
        }

        /// <summary>
        /// Insert before anchor component
        /// </summary>
        /// <param name="component">New component</param>
        /// <param name="anchor">Anchor component</param>
        public void InsertBefore(ComponentBase component, ComponentBase anchor)
        {
            CheckComponentPair(component, anchor);

            if (component.Class == ComponentBase.ComponentClass.Output)
            {
                throw new Exception("Output components can only be inserted with straight connection to the power rail");
            }
            else
            {
                component.LeftLide = anchor.LeftLide;
                anchor.LeftLide = component.RightLide;

                _Components.Insert(_Components.IndexOf(anchor), component);

                if (anchor is ShortCircuit)
                {
                    component.RightLide = anchor.RightLide;
                    _Components.Remove(anchor);
                }

                component.RightLide.Root = component;
            }
        }

        /// <summary>
        /// Insert after anchor component
        /// </summary>
        /// <param name="component">New component</param>
        /// <param name="anchor">Anchor component</param>
        public void InsertAfter(ComponentBase component, ComponentBase anchor)
        {
            CheckComponentPair(component, anchor);

            if (anchor.RightLide.Root == null) anchor.RightLide.Root = anchor;

            if (component.Class == ComponentBase.ComponentClass.Output)
            {
                if (anchor.Class == ComponentBase.ComponentClass.Output) throw new Exception("Can not insert output component in series with an output component");

                component.LeftLide = anchor.RightLide;
                component.RightLide = GroundRail;
                _Components.Add(component);
            }
            else
            {
                if (anchor.Class == ComponentBase.ComponentClass.Output) throw new Exception("Can not insert input component after an output component");

                component.RightLide = anchor.RightLide;
                anchor.RightLide = component.LeftLide;
                anchor.RightLide.Root = anchor;

                if (component.RightLide.Root == anchor) component.RightLide.Root = component;

                _Components.Insert(_Components.IndexOf(anchor) + 1, component);

                if (anchor is ShortCircuit)
                {
                    component.LeftLide = anchor.LeftLide;
                    _Components.Remove(anchor);
                }
            }
        }

        #endregion Insert Functions

        #region Delete Functions
        /// <summary>
        /// Delete compoenent from the rung
        /// </summary>
        /// <param name="component">Component to be deleted</param>
        public void Remove(ComponentBase component)
        {
            if (!_Components.Contains(component)) throw new Exception("Component not inserted in current Rung");

            int nextComponentIndex = _Components.IndexOf(component) + 1;

            if (component.RightLide.Root == component) //Component is owner of the Right node
            {
                if (nextComponentIndex < _Components.Count) //is not the last component in rung
                {
                    if (_Components[nextComponentIndex].RightLide == component.RightLide)// Next component conected in parallel
                    {
                        component.RightLide.Root = _Components[nextComponentIndex];
                    }
                    else //next component in series
                    {
                        //In case of parallel components
                        for (int c = nextComponentIndex; c < _Components.Count && _Components[c].LeftLide == component.RightLide; c++)
                        {
                            _Components[c].LeftLide = component.LeftLide;
                        }
                    }
                }
            }

            _Components.Remove(component);
        }

        /// <summary>
        /// Clear Rung (delete all components)
        /// </summary>
        public void Clear()
        {
            _Components.Clear();
        }
        #endregion Delete Functions

        #endregion Functions

        #region Constructors
        /// <summary>
        /// Default Builder
        /// </summary>
        public Rung()
        {
            Components = new ObservableCollection<ComponentBase>();
            PowerRail = new Node();
            GroundRail = new Node();
            PowerRail.LogicLevel = true;
        }
        #endregion Constructors

        #region Internal Data
        ObservableCollection<ComponentBase> _Components;
        Node PowerRail;
        Node GroundRail;

        string _Comment;

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion Internal Data
    }
}
