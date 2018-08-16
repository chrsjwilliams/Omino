using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using EasingEquations;

namespace BeatManagement
{
   public class Clock : MonoBehaviour
   {
      #region /// Public Variables ///
      public ClockEventManager eventManager;
      protected Clock() { }

      public void Init(double bpm)
      {
         BPM = bpm;
      }

      public double BPM;
      public string MBT;
      public double StartDelay;
      public double LatencyCompensation = 0;
      
      public double _secondsPerMeasure;

      public int beatsPerMeasure
      {
         get { return _beatsPerMeasure; }
      }
      
      public enum BeatValue
      {
         ThirtySecond = 1,
         Sixteenth = 2,
         Eighth = 4,
         Quarter = 8,
         Half = 16,
         Whole = 32,
         Measure = 33,
         Max = 34
      };

      public class BeatArgs
      {
         public BeatValue BeatVal;
         public int BeatCount;
         public double BeatTime;
         public double NextBeatTime;
         public bool[] BeatMask = new bool[(int)BeatValue.Max];

         public BeatArgs(BeatValue beatVal, int beatCount, double beatTime, double nextBeatTime, bool[] beatMask)
         {
            BeatVal = beatVal;
            BeatCount = beatCount;
            BeatTime = beatTime;
            NextBeatTime = nextBeatTime;
            BeatMask = beatMask;
         }
      }

      public delegate void BeatEvent(BeatArgs args);

      public event BeatEvent Tick;
      public event BeatEvent Beat;
      
      #endregion
      
      #region /// Private Variables ///
      private int _beatsPerMeasure = 4;
      private BeatValue _unitOfTempo = BeatValue.Quarter;
      
      private bool[] _beatMask = new bool[(int)BeatValue.Max];
      
      private int _thirtySecondCount;
      private int _sixteenthCount;
      private int _eighthCount;
      private int _quarterCount;
      private int _halfCount;
      private int _wholeCount;
      private int _measureCount;
      
      private double _thirtySecondLength;
      private double _sixteenthLength;
      private double _eighthLength;
      private double _quarterLength;
      private double _halfLength;
      private double _wholeLength;
      private double _measureLength;
      
      private double _nextThirtySecond = System.Double.MaxValue;
      private double _nextMeasure;
      private double _nextSixteenth;
      private double _nextEighth;
      private double _nextQuarter;
      private double _nextHalf;
      private double _nextWhole;

      // private List<double> latency = new List<double>();
      
      #endregion
      
      #region /// Monobehavior Functions ///
      void Start()
      {
         _beatMask = new bool[(int)BeatValue.Max];
         _InitializeBPM(BPM);
         eventManager = new ClockEventManager();
      }

      void Update()
      {
         if (AudioSettings.dspTime >= _nextThirtySecond - LatencyCompensation)
         {
            _UpdateBeats();
         }
         Array.Clear(_beatMask, 0, _beatMask.Length);

         int beatCount = 0;
         switch (_unitOfTempo)
         {
            case (BeatValue.Half) : beatCount = _halfCount;
               break;
            case (BeatValue.Quarter) : beatCount = _quarterCount;
               break;
            case (BeatValue.Eighth) : beatCount = _eighthCount;
               break;
            case (BeatValue.Sixteenth) : beatCount = _sixteenthCount;
               break;
            case (BeatValue.ThirtySecond) : beatCount = _thirtySecondCount;
               break;
         }
         
         int beats = 1 + ((beatCount - 1) % _beatsPerMeasure);
         MBT = _measureCount.ToString() + ":" + beats.ToString() + ":" + ((_thirtySecondCount % 8)+1).ToString();

         //double rollav = _RollingAverage(latency);
         //if (rollav != 0.0)
         //Debug.Log("Average Drift of Last 25 Notes: " + (rollav * 1000).ToString().Remove(5) + "ms");
      }
      
      #endregion

      #region /// Public Access Functions ///
      public void SetBPM(int newBPM)
      {
         double BPMdbl = (double)newBPM;
         _InitializeBPM(BPMdbl);
      }

      public void SetBPM(float newBPM)
      {
         double BPMdbl = (double)newBPM;
         _InitializeBPM(BPMdbl);
      }

      public void SetBPM(double NewBPM)
      {
         _InitializeBPM((NewBPM));
      }

      public void SetTimeSignature(int beatsPerMeasure, BeatValue unitOfTempo)
      {
         _beatsPerMeasure = beatsPerMeasure;
         _unitOfTempo = unitOfTempo;
         _InitializeBPM(BPM);
      }
      
      public void ClearEvents()
      {
         eventManager = new ClockEventManager();
      }

      #endregion
      
      #region /// Private Functions ///

      private void _InitializeBPM(double NewBPM)
      {
         _ResetBeatCounts();
         BPM = NewBPM;
         _secondsPerMeasure = 60 / BPM * _beatsPerMeasure;
         _SetLengths();
         _FirstBeat();
      }

      private void _SetLengths()
      {
         _measureLength = _secondsPerMeasure;
         float beatVal = 32 / (int)_unitOfTempo;
         
         _thirtySecondLength = _measureLength / _beatsPerMeasure / (32 / beatVal);
         _sixteenthLength = _measureLength / _beatsPerMeasure / (16 / beatVal);
         _eighthLength = _measureLength / _beatsPerMeasure / (8 / beatVal);
         _quarterLength = _measureLength / _beatsPerMeasure / (4 / beatVal);
         _halfLength = _measureLength / _beatsPerMeasure / (2 / beatVal);
         _wholeLength = _measureLength / _beatsPerMeasure / (1 / beatVal);
      }

      private void _FirstBeat()
      {
         double time = AudioSettings.dspTime + StartDelay;
         _nextThirtySecond = time + _thirtySecondLength;
         _nextSixteenth = time + _sixteenthLength;
         _nextEighth = time + _eighthLength;
         _nextQuarter = time + _quarterLength;
         _nextHalf = time + _halfLength;
         _nextWhole = time + _wholeLength;
         _nextMeasure = time + _measureLength;
      }

      private void _ResetBeatCounts()
      {
         _thirtySecondCount = 0;
         _sixteenthCount = 1;
         _eighthCount = 1;
         _quarterCount = 1;
         _halfCount = 1;
         _wholeCount = 1;
         _measureCount = 1;
      }
      
      private void _UpdateBeats()
      {
         _thirtySecondCount++;
         _BuildBeatMask();
         if (Tick != null)
            Tick(new BeatArgs(BeatValue.ThirtySecond, _thirtySecondCount, _nextThirtySecond, _nextThirtySecond + _thirtySecondLength, _beatMask));
         
         eventManager.Fire(new ThirtySecond(_thirtySecondCount));
         
         if (Beat != null)
            Beat(new BeatArgs(BeatValue.ThirtySecond, _thirtySecondCount, _nextThirtySecond, _nextThirtySecond + _thirtySecondLength, _beatMask));
            
         if (_unitOfTempo == BeatValue.ThirtySecond)
            eventManager.Fire(new Beat(_thirtySecondCount));
         
         //latency.Add(AudioSettings.dspTime - _nextThirtySecond); //benchmarking stuff
         _nextThirtySecond += _thirtySecondLength;
         
         if (_beatMask[(int)BeatValue.Sixteenth])
         {
            _sixteenthCount++;
            eventManager.Fire(new Sixteenth(_sixteenthCount));
            if (Beat != null) 
               Beat(new BeatArgs(BeatValue.Sixteenth, _sixteenthCount, _nextSixteenth, _nextSixteenth + _sixteenthLength, _beatMask));
               
            if (_unitOfTempo == BeatValue.Sixteenth)
               eventManager.Fire(new Beat(_sixteenthCount));
            
            _nextSixteenth += _sixteenthLength;
         }
         if (_beatMask[(int)BeatValue.Eighth])
         {
            _eighthCount++;
            eventManager.Fire(new Eighth(_eighthCount));
            if (Beat != null)
               Beat(new BeatArgs(BeatValue.Eighth, _eighthCount, _nextEighth,  _nextEighth + _eighthLength, _beatMask));
               
            if (_unitOfTempo == BeatValue.Eighth)
               eventManager.Fire(new Beat(_eighthCount));
            
            _nextEighth += _eighthLength;
         }
         if (_beatMask[(int)BeatValue.Quarter])
         {
            _quarterCount++;
            eventManager.Fire(new Quarter(_quarterCount));
            
            if (Beat != null)
               Beat(new BeatArgs(BeatValue.Quarter, _quarterCount, _nextQuarter, _nextQuarter + _quarterLength,
                  _beatMask));
               
            if (_unitOfTempo == BeatValue.Quarter)
               eventManager.Fire(new Beat(_quarterCount));
            
            _nextQuarter += _quarterLength;
         }
         if (_beatMask[(int)BeatValue.Half])
         {
            _halfCount++;
            eventManager.Fire(new Half(_halfCount));
            if (Beat != null) 
               Beat(new BeatArgs(BeatValue.Half, _halfCount, _nextHalf, _nextHalf + _halfLength, _beatMask));
            
            if (_unitOfTempo == BeatValue.Half)
               eventManager.Fire(new Beat(_halfCount));
            
            _nextHalf += _halfLength;
         }
         if (_beatMask[(int) BeatValue.Whole])
         {
            _wholeCount++;
            eventManager.Fire(new Whole(_wholeCount));
            _nextWhole += _wholeLength;
         }
         if (_beatMask[(int)BeatValue.Measure])
         {
            _measureCount++;
            eventManager.Fire(new Measure(_measureCount));
            _nextMeasure += _measureLength;
         }
      }

      private void _BuildBeatMask()
      {
         if (_thirtySecondCount % ((int) _unitOfTempo * _beatsPerMeasure) == 0)
            _beatMask[(int) BeatValue.Measure] = true;
         
         _beatMask[(int)BeatValue.ThirtySecond] = true;
         if (_thirtySecondCount % 2 != 0) return;
         _beatMask[(int)BeatValue.Sixteenth] = true;
         if (_thirtySecondCount % 4 != 0) return;
         _beatMask[(int)BeatValue.Eighth] = true;
         if (_thirtySecondCount % 8 != 0) return;
         _beatMask[(int)BeatValue.Quarter] = true;
         if (_thirtySecondCount % 16 != 0) return;
         _beatMask[(int)BeatValue.Half] = true;
         if (_thirtySecondCount % 32 != 0) return;
         _beatMask[(int)BeatValue.Whole] = true;
      }
      
      private double _RollingAverage(List<double> values)
      {
         int periodLength = 25;
         if (values.Count - 1 < periodLength) return 0.0;
         var temp = Enumerable
            .Range(0, values.Count - periodLength)
            .Select(n => values.Skip(n).Take(periodLength).Average())
            .ToList();
         return temp[temp.Count - 1];
      }
      
      #endregion

      #region /// Public Helper Functions ///
      
      public void SyncFunction(System.Action callback, BeatValue beatValue = BeatValue.Measure)
      {
         StartCoroutine(YieldForSync(callback, beatValue));
      }

      IEnumerator YieldForSync(System.Action callback, BeatValue beatValue)
      {
         int startCount = _thirtySecondCount % 32;
         bool isStartNote = true;
         bool waiting = true;
         while (waiting)
         {
            isStartNote = (isStartNote && startCount == (_thirtySecondCount % 32));
            if (isStartNote)
               yield return false;
            isStartNote = false;
            if (beatValue == BeatValue.ThirtySecond || (_thirtySecondCount % 32) % (int)beatValue == 1)
               waiting = false;
            else
               yield return false;
         }
         callback();
      }

      public double ReturnAtNext(BeatValue next)
      {
         switch (next)
         {
            case (BeatValue.Measure) :
               return AtNextMeasure();
            case (BeatValue.Whole) :
               return AtNextWhole();
            case (BeatValue.Half) :
               return AtNextHalf();
            case (BeatValue.Quarter) :
               return AtNextQuarter();
            case (BeatValue.Eighth) :
               return AtNextEighth();
            case (BeatValue.Sixteenth) :
               return AtNextSixteenth();
            case (BeatValue.ThirtySecond) :
               return AtNextThirtySecond();
         }

         return 0.0d;
      }
      
      //Helper functions for cueing things to play (usually audio) at the next available interval
      //For playing audio, use AudioSource.PlayScheduled(AtNextThirtySecond());
      public double AtNextThirtySecond()
      {
         return _nextThirtySecond;
      }

      public double AtNextSixteenth()
      {
         return _nextSixteenth;
      }

      public double AtNextEighth()
      {
         return _nextEighth;
      }

      public double AtNextQuarter()
      {
         return _nextQuarter;
      }

      public double AtNextBeat()
      {
         return ReturnAtNext(_unitOfTempo);
      }

      public double AtNextHalf()
      {
         return _nextHalf;
      }

      public double AtNextWhole()
      {
         return _nextWhole;
      }

      public double AtNextMeasure()
      {
         return _nextMeasure;
      }

      //Helper functions for timing things like animations, etc. to the beatclock
      //These are casted to floats as most animations, tweens, etc. take float values for time
      public float ThirtySecondLength()
      {
         return (float) _thirtySecondLength;
      }

      public float SixteenthLength()
      {
         return (float) _sixteenthLength;
      }

      public float EighthLength()
      {
         return (float) _eighthLength;
      }

      public float QuarterLength()
      {
         return (float) _quarterLength;
      }

      public float BeatLength()
      {
         switch (_unitOfTempo)
         {
            case (BeatValue.Half) : return (float) _halfLength;
            case (BeatValue.Eighth) : return (float) _eighthLength;
            case (BeatValue.Sixteenth) : return (float) _sixteenthLength;
            case (BeatValue.ThirtySecond) : return (float) _thirtySecondLength;
            default: return (float) _quarterLength;
         }
      }

      public float HalfLength()
      {
         return (float) _halfLength;
      }

      public float WholeLength()
      {
         return (float) _wholeLength;
      }
    
      public float MeasureLength()
      {
         return (float) _measureLength;
      }
      
      #endregion
   }

   #region /// Event Manager For Repeated Actions on Beats ///
   public class ClockEventManager
   {
      private Dictionary<System.Type, BeatEvent.Handler> registered_handlers;

      public ClockEventManager() {
         registered_handlers = new Dictionary<System.Type, BeatEvent.Handler>();
      }

      public void Register<T>(BeatEvent.Handler handler) where T : BeatEvent {
         System.Type type = typeof(T);
         if (registered_handlers.ContainsKey(type)) {
            if (!IsEventHandlerRegistered(type, handler))
               registered_handlers[type] += handler;         
         } else {
            registered_handlers.Add(type, handler);         
         }     
      } 

      public void Unregister<T>(BeatEvent.Handler handler) where T : BeatEvent {         
         System.Type type = typeof(T);         
         BeatEvent.Handler handlers;         
         if (registered_handlers.TryGetValue(type, out handlers)) {             
            handlers -= handler;             
            if (handlers == null) {                 
               registered_handlers.Remove(type);             
            } else {
               registered_handlers[type] = handlers;             
            }         
         }     
      }      
		
      public void Fire(BeatEvent e) 
      {         
         System.Type type = e.GetType();         
         BeatEvent.Handler handlers;   
		
         if (registered_handlers.TryGetValue(type, out handlers)) {             
            handlers(e);
         }     
      } 

      public bool IsEventHandlerRegistered (System.Type type_in, System.Delegate prospective_handler)
      {   
         foreach (System.Delegate existingHandler in registered_handlers[type_in].GetInvocationList()) {
            if (existingHandler == prospective_handler) {
               return true;
            }
         }
         return false;
      }

   }

   public abstract class BeatEvent {
      public readonly float creation_time;
      public readonly int beatCount;
	
      public BeatEvent (int beatCount)
      {
         creation_time = Time.time;
         this.beatCount = beatCount;
      }

      public delegate void Handler (BeatEvent e);
   }

   public class Beat : BeatEvent { public Beat(int beatCount) : base(beatCount) { } }
   public class ThirtySecond : BeatEvent { public ThirtySecond(int beatCount) : base(beatCount) { } }
   public class Sixteenth : BeatEvent { public Sixteenth(int beatCount) : base(beatCount) { } }
   public class Eighth : BeatEvent { public Eighth(int beatCount) : base(beatCount) { } }
   public class Quarter : BeatEvent { public Quarter(int beatCount) : base(beatCount) { } }
   public class Half : BeatEvent { public Half(int beatCount) : base(beatCount) { } }
   public class Whole : BeatEvent { public Whole(int beatCount) : base(beatCount) { } }
   public class Measure : BeatEvent { public Measure(int beatCount) : base(beatCount) { } } 
   
   #endregion
}