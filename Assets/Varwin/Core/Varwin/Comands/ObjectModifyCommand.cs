using System;
using UnityEngine;
using Varwin.Data.ServerData;
using Varwin.Models.Data;

namespace Varwin.Commands
{
    public class ModifyCommand : Command
    {
        private readonly TransformDT _saveTransformDt;
        private readonly TransformDT _newTransformDt;
        private GameObject _gameObject;
        private readonly JointData _saveJointData;
        private readonly ObjectController _objectController;
        private readonly Action<ObjectController> _callback;

        /// <summary>
        /// Modify object command
        /// </summary>
        /// <param name="objectController">Controller</param>
        /// <param name="gameObject">Modifided gameObject</param>
        /// <param name="newTransform">New RootGameObject transform</param>
        /// <param name="saveTransform">Saved transform to redo action</param>
        /// <param name="jointData">Saved joints</param>
        public ModifyCommand(ObjectController objectController, GameObject gameObject, TransformDT newTransform = null, TransformDT saveTransform = null, JointData jointData = null, Action<ObjectController> callback = null)
        {
            //ToDo сохратять так же ObjectId
            _saveTransformDt = saveTransform;
            _newTransformDt = newTransform;
            _gameObject = gameObject;
            _objectController = objectController;
            _saveJointData = jointData;
            _callback = callback;
            SaveObject(objectController);
            CommandsManager.AddCommand(this);
        }

        protected override void Execute()
        {
            if (_gameObject == null)
            {
                //ToDo искать дочернийц объект по ObjectId
                _gameObject = GetObject().RootGameObject;
            }

            _newTransformDt?.ToTransformUnity(_gameObject.transform);
            _callback?.Invoke(_objectController);
        }

        protected override void Undo()
        {
            if (_gameObject == null)
            {
                _gameObject = GetObject().RootGameObject;
            }

            var joint = _gameObject.GetComponent<JointBehaviour>();

            if (joint != null)
            {
                joint.MakeConnectionsChild();
            }

            _saveTransformDt?.ToTransformUnity(_gameObject.transform);
            _callback?.Invoke(_objectController);

            if (joint != null)
            {
                joint.RestoreParents();
            }

            if (_saveJointData == null)
            {
                return;
            }

            Helper.ReloadJointConnections(_objectController, _saveJointData);
        }
    }
}
