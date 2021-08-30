using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityTools;
using UnityTools.Common;

namespace FluidSPH3D
{
	public class EmitterController : MonoBehaviour
	{
        public class EmitterContainer : GPUContainer
        {
			[Shader(Name = "_EmitterBuffer")] public GPUBufferVariable<EmitterGPUData> emitterBuffer = new GPUBufferVariable<EmitterGPUData>();
        }
        public EmitterContainer EmitterGPUData => this.emitterContainer;
		public int CurrentParticleEmit => this.emitters.Sum(e => e.particlePerEmit);
		protected const int MAX_NUM_EMITTER = 128;
		protected EmitterConfigure configure;
		protected EmitterConfigure Configure => this.configure ??= this.gameObject.FindOrAddTypeInComponentsAndChildren<EmitterConfigure>();
		protected List<IEmitter> emitters = new List<IEmitter>();
		protected EmitterContainer emitterContainer = new EmitterContainer();

		public void Init()
		{
            this.emitters.Clear();
            this.emitters.AddRange(this.gameObject.GetComponentsInChildren<IEmitter>());

			this.Configure.Initialize();
			foreach (var ec in this.Configure.D.emitters)
			{
				var go = new GameObject(ec.name);
				go.transform.parent = this.gameObject.transform;
				var e = go.AddComponent<Emitter>();
                e.Init(ec);

                this.emitters.Add(e);
			}

            this.emitterContainer.emitterBuffer.InitBuffer(MAX_NUM_EMITTER, true, true);
		}
		public void Deinit()
		{
			this.emitterContainer?.Release();
		}
		protected void UpdateEmitterData()
		{
			var ecount = 0;
			var eCPU = this.emitterContainer.emitterBuffer.CPUData;
			foreach (var e in this.emitters)
			{
				eCPU[ecount].enabled = e.IsActive;
				eCPU[ecount].particlePerEmit = Mathf.CeilToInt(e.particlePerEmit * UnityEngine.Random.value);
				eCPU[ecount].localToWorld = e.Space.TRS;
				ecount++;
			}
			while (ecount < MAX_NUM_EMITTER) eCPU[ecount++].enabled = false;
		}
		protected void Update()
		{
			this.UpdateEmitterData();
		}
		protected void OnEnable()
		{
			// this.Init();
		}
		protected void OnDisable()
		{
			// this.Deinit();
		}
	}
}
