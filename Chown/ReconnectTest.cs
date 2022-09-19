using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using OzricEngine;
using OzricEngine.Nodes;
using OzricEngine.Values;
using ValueType = OzricEngine.Values.ValueType;

internal class Program
{
    public const string llat =
        "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiI5NjIyODE0NmFmMmQ0YTVmOWZiZmNiNDRmNTY0ZGQ4NSIsImlhdCI6MTYzNzAwMzc4OCwiZXhwIjoxOTUyMzYzNzg4fQ.YeZGrm3Shnx5Zu8MedejVB61t2GWWr4gU0MqIqb0cXY";

    //private static ColorValue dimOrange = new ColorHS(0.1f, 1f, 0.35f);
    private static ColorValue dimOrange = new ColorXY(0.5772f, 0.3941f, 97 / 255f);
    private static ColorValue veryDimOrange = new ColorHS(18 / 360f, 1, 76 / 255f);
    private static ColorValue warmOrange = new ColorHS(28.251f / 360f, 87.451f / 100f, 97 / 255f);
    private static ColorValue brightWhite = new ColorTemp(156, 1f);
    private static ColorValue warmBrightWhite = new ColorTemp(375, 0.35f);
    private static ColorValue warmWhite = new ColorHS(39f / 360f, 0.5f, 0.5f);
//    private static ColorValue warmWhite = brightWhite;
    private static ColorValue off = new ColorHS(0, 0, 0);

    class VerboseObject : OzricObject
    {
        public override string Name => "Logger";

        public void ExampleLog(LogLevel level)
        {
            minLogLevel = level;            
            Log(level, "Log level {0}", level);
        }
    }

    public static async Task Main(string[] args)
    {
        var dir = "S:\\Work\\Home\\OzricAddon\\Ozric\\Chown";
        
        try
        {
            var graph = new Graph();

            //HallJustLights(graph);
            Hall(graph);
            Kitchen(graph);

            var graphJson = Json.Prettify(Json.Serialize(graph));
            Console.WriteLine(graphJson);
            await File.WriteAllTextAsync($"{dir}\\chown.json", graphJson);
            
            graph = Json.Deserialize<Graph>(graphJson);

            var tx = new MessageWriter($"{dir}\\comms-tx.msg");
            var rx = new MessageWriter($"{dir}\\comms-rx.msg");

            using (var connection = new Comms(llat))
            {
                await connection.Authenticate();

                connection.OnSend(message => tx.Write(message));
                connection.OnReceive(message => rx.Write(message));
                
                //await HallColorTests3(connection);
                //await SetSpots(connection);

                await connection.Send(new ClientGetStates());
                var states = await connection.Receive<ServerGetStates>() ?? throw new InvalidOperationException();
                
                var statesJson = Json.Prettify(Json.Serialize(states));
                Console.WriteLine(statesJson);
                await File.WriteAllTextAsync($"{dir}\\states.json", statesJson);

                var home = new Home(states.result);
                await File.WriteAllTextAsync($"{dir}\\home.json", Json.Prettify(Json.Serialize(home)));

                var engine = new Engine(home, graph, connection);

                //_ = HallFlashTest(graph);

                await engine.MainLoop();

                // await AllLights(engine);
                
                // await LightTest(engine);

                // await Sensor1(engine);
                
                // await TogglePanasonic(states.result, connection);

                // await engine.MainLoop();

                //await ProcessEvents(engine);
            }
        }
        catch (Exception e)
        {
            Console.Write(e);
        }
    }

    private static async Task HallFlashTest(Graph graph)
    {
        while (true)
        {
            Console.WriteLine("orange");
            await SetHallLights(graph, veryDimOrange);
            
            Console.WriteLine("white");
            await SetHallLights(graph, brightWhite);
        }
    }

    private static async Task SetHallLights(Graph graph, ColorValue color)
    {
        for (int i = 1; i <= 4; i++)
            graph.nodes[$"hall-light-{i}"].inputs[0].SetValue(color);

        await Task.Delay(3000);
    }

    private static async Task SetSpots(Comms comms)
    {
        for (int i = 1; i <= 4; i++)
        {
            var entityID = $"light.hue_color_spot{i}";

            var callService = new ClientCallService
            {
                domain = "light",
                service = "turn_on",
                target = new Attributes()
                {
                    { "entity_id", new List<string> { entityID } }
                },
                service_data = new Attributes()
                {
                    { "brightness", 89 },
                    { "xy_color", new List<Single> { 0.58325046f, 0.4023088f } }
                }
            };

            await comms.Send(callService);
            await comms.Receive<ServerResult>();
        }
        
        await Task.Delay(5000);
    }

    private static async Task HallColorTests0(Comms comms)
    {
        while (true)
        {
            Console.WriteLine("HS");

            await HallLights(comms, attributes => attributes["hs_color"] = new List<int> { 28, 87 }); 
            await Task.Delay(100);
            await HallLights(comms, attributes => attributes["brightness"] = 89); 

            await Task.Delay(5000);

            Console.WriteLine("TEMP");
            
            await HallLights(comms, attributes => attributes["color_temp"] = 156); 
            await Task.Delay(100);
            await HallLights(comms, attributes => attributes["brightness"] = 50); 

            await Task.Delay(5000);

            Console.WriteLine("OFF");
            
            await HallLights(comms, attributes => { }, false); 

            await Task.Delay(5000);
        }
    }

    private static async Task HallColorTests1(Comms comms)
    {
        while (true)
        {
            Console.WriteLine("HS");

            await HallLights(comms, attributes => attributes["brightness"] = 89); 
            await Task.Delay(100);
            await HallLights(comms, attributes => attributes["hs_color"] = new List<int> { 28, 87 }); 

            await Task.Delay(5000);

            Console.WriteLine("TEMP");
            
            await HallLights(comms, attributes => attributes["brightness"] = 50); 
            await Task.Delay(100);
            await HallLights(comms, attributes => attributes["color_temp"] = 156); 

            await Task.Delay(5000);

            Console.WriteLine("OFF");
            
            await HallLights(comms, attributes => { }, false); 

            await Task.Delay(5000);
        }
    }

    private static async Task HallColorTests2(Comms comms)
    {
        while (true)
        {
            Console.WriteLine("HS");

            await HallLights(comms, attributes =>
            {
                attributes["brightness"] = 89;
                attributes["hs_color"] = new List<int> { 28, 87 };
            }); 

            await Task.Delay(5000);

            Console.WriteLine("TEMP");
            
            await HallLights(comms, attributes =>
            {
                attributes["brightness"] = 50;
                attributes["color_temp"] = 156; 
            }); 

            await Task.Delay(5000);

            Console.WriteLine("OFF");
            
            await HallLights(comms, attributes => { }, false); 

            await Task.Delay(5000);
        }
    }
    
    
    private static async Task HallColorTests3(Comms comms)
    {
        while (true)
        {
            Console.WriteLine("HS");

            await HallLightsSpam(comms, attributes =>
            {
                attributes["brightness"] = 89;
                attributes["hs_color"] = new List<int> { 28, 87 };
            }); 

            await Task.Delay(5000);

            Console.WriteLine("TEMP");
            
            await HallLightsSpam(comms, attributes =>
            {
                attributes["brightness"] = 50;
                attributes["color_temp"] = 156; 
            }); 

            await Task.Delay(5000);

            Console.WriteLine("OFF");
            
            await HallLights(comms, attributes => { }, false); 

            await Task.Delay(5000);
        }
    }

    private static async Task HallLightsSpam(Comms comms, Action<Attributes> setServiceData, bool on = true)
    {
        for (int i = 0; i < 2; i++)
        {
            await HallLights(comms, attributes =>
            {
                setServiceData(attributes);
                if ((i&1) == 0)
                {
                    attributes.Remove("brightness");
                }
                else
                {
                    attributes.Remove("hs_color");
                    attributes.Remove("color_temp");
                }
            }, on);
            await Task.Delay(100);
        }
    }

    private static async Task HallLights(Comms comms, Action<Attributes> setServiceData, bool on = true)
    {
        var callService = new ClientCallService
        {
            domain = "light",
            service = on ? "turn_on" : "turn_off",
            target = new Attributes
            {
                { "entity_id", new List<string> { "light.hall_1", "light.hall_2", "light.hall_3", "light.hall_4" } }
            },
            service_data = new Attributes()
        };
        
        setServiceData(callService.service_data);

        await comms.Send(callService);
        await comms.Receive<ServerResult>();
    }

    private static async Task LightTest(Engine engine)
    {
        var light = new Light("kitchen-light-1", "light.hue_color_spot_1");

        engine.graph.AddNode(light);

        await engine.MainLoop();
    }

    private static async Task Sensor1(Engine engine)
    {
        engine.graph.AddNode(new Sensor("kitchen-sensor", "binary_sensor.tuyatec_zn9wyqtr_rh3040_bbf0cbfe_ias_zone"));

        await engine.MainLoop();
    }

    private static void Hall(Graph graph)
    {
        graph.AddNode(new Sensor("hall-sensor-1", "binary_sensor.tuyatec_zn9wyqtr_rh3040_6cf5cbfe_ias_zone"));
        graph.AddNode(new Sensor("hall-sensor-2", "binary_sensor.tuyatec_zn9wyqtr_rh3040_00480cfe_ias_zone"));
        graph.AddNode(new IfAny("hall-sensors", "input-1", "input-2"));

        var hallModes = new DayPhases("hall-modes");
        hallModes.AddPhase(new DayPhases.PhaseStart(new Mode("dark"), DayPhases.SunPhase.Midnight, -2 * 60 * 60));
        hallModes.AddPhase(new DayPhases.PhaseStart(new Mode("day"), DayPhases.SunPhase.Midnight, 6 * 60 * 60));
        hallModes.AddPhase(new DayPhases.PhaseStart(new Mode("evening"), DayPhases.SunPhase.Setting, -30 * 60));
        
        var hallColours = new ModeSwitch("hall-colours");
        hallColours.AddOutput("colour-on", ValueType.Color);
        hallColours.AddOutput("colour-off", ValueType.Color);
        hallColours.AddModeValues("dark", ("colour-on", warmBrightWhite), ("colour-off", veryDimOrange));
        hallColours.AddModeValues("day", ("colour-on", warmBrightWhite), ("colour-off", veryDimOrange));
        hallColours.AddModeValues("evening", ("colour-on", warmBrightWhite), ("colour-off", warmOrange));
        
        var hallSwitch = new BooleanChoice("hall-switch", ValueType.Color);

        graph.AddNode(hallModes);
        graph.AddNode(hallColours);
        graph.AddNode(hallSwitch);

        graph.AddNode(new Light("hall-light-1", "light.hall_1"));
        graph.AddNode(new Light("hall-light-2", "light.hall_2"));
        graph.AddNode(new Light("hall-light-3", "light.hall_3"));
        graph.AddNode(new Light("hall-light-4", "light.hall_4"));
        
        graph.Connect("hall-modes", "mode", "hall-colours", "mode");
        graph.Connect("hall-colours", "colour-on", "hall-switch", "on");
        graph.Connect("hall-colours", "colour-off", "hall-switch", "off");
        graph.Connect("hall-sensor-1", "activity", "hall-sensors", "input-1");
        graph.Connect("hall-sensor-2", "activity", "hall-sensors", "input-2");
        graph.Connect("hall-sensors", "output", "hall-switch", "switch");
        graph.Connect("hall-switch", "output", "hall-light-1", "color");
        graph.Connect("hall-switch", "output", "hall-light-2", "color");
        graph.Connect("hall-switch", "output", "hall-light-3", "color");
        graph.Connect("hall-switch", "output", "hall-light-4", "color");
    }

    private static void HallJustLights(Graph graph)
    {
        graph.AddNode(new Light("hall-light-1", "light.hall_1"));
        graph.AddNode(new Light("hall-light-2", "light.hall_2"));
        graph.AddNode(new Light("hall-light-3", "light.hall_3"));
        graph.AddNode(new Light("hall-light-4", "light.hall_4"));
    }

    private static void Kitchen(Graph graph)
    {
        graph.AddNode(new Sensor("kitchen-sensor-1", "binary_sensor.tuyatec_zn9wyqtr_rh3040_bbf0cbfe_ias_zone"));
        graph.AddNode(new Sensor("kitchen-sensor-2", "binary_sensor.tuyatec_zn9wyqtr_rh3040_090ecafe_ias_zone"));
        graph.AddNode(new IfAny("kitchen-sensors", "input-1", "input-2"));

        var kitchenModes = new DayPhases("kitchen-modes");
        kitchenModes.AddPhase(new DayPhases.PhaseStart(new Mode("bedtime"), DayPhases.SunPhase.Midnight, - 10 * 60));
        kitchenModes.AddPhase(new DayPhases.PhaseStart(new Mode("daytime"), DayPhases.SunPhase.Midnight, 6 * 60 * 60));
        kitchenModes.AddPhase(new DayPhases.PhaseStart(new Mode("evening"), DayPhases.SunPhase.Setting, -30 * 60));
        kitchenModes.AddPhase(new DayPhases.PhaseStart(new Mode("night"), DayPhases.SunPhase.Midnight, -4 * 60 * 60));

        var kitchenColours = new ModeSwitch("kitchen-colours");
        kitchenColours.AddOutput("colour-on", ValueType.Color);
        kitchenColours.AddOutput("colour-off", ValueType.Color);
        kitchenColours.AddModeValues("bedtime", ("colour-on", dimOrange), ("colour-off", off));
        kitchenColours.AddModeValues("daytime", ("colour-on", brightWhite), ("colour-off", off));
        kitchenColours.AddModeValues("evening", ("colour-on", brightWhite), ("colour-off", warmOrange));
        kitchenColours.AddModeValues("night", ("colour-on", warmWhite), ("colour-off", dimOrange));

        var kitchenSwitch = new BooleanChoice("kitchen-switch", ValueType.Color);

        graph.AddNode(kitchenModes);
        graph.AddNode(kitchenColours);
        graph.AddNode(kitchenSwitch);

        graph.AddNode(new Light("kitchen-light-1", "light.hue_color_spot_1"));
        graph.AddNode(new Light("kitchen-light-2", "light.hue_color_spot_2"));
        graph.AddNode(new Light("kitchen-light-3", "light.hue_color_spot_3"));
        graph.AddNode(new Light("kitchen-light-4", "light.hue_color_spot_4"));

        graph.Connect("kitchen-modes", "mode", "kitchen-colours", "mode");
        graph.Connect("kitchen-colours", "colour-on", "kitchen-switch", "on");
        graph.Connect("kitchen-colours", "colour-off", "kitchen-switch", "off");
        graph.Connect("kitchen-sensor-1", "activity", "kitchen-sensors", "input-1");
        graph.Connect("kitchen-sensor-2", "activity", "kitchen-sensors", "input-2");
        graph.Connect("kitchen-sensors", "output", "kitchen-switch", "switch");
        graph.Connect("kitchen-switch", "output", "kitchen-light-1", "color");
        graph.Connect("kitchen-switch", "output", "kitchen-light-2", "color");
        graph.Connect("kitchen-switch", "output", "kitchen-light-3", "color");
        graph.Connect("kitchen-switch", "output", "kitchen-light-4", "color");
    }

    private static void HallSensors(Graph graph)
    {
        graph.AddNode(new Sensor("sensor-01", "binary_sensor.tuyatec_zn9wyqtr_rh3040_090ecafe_ias_zone"));
        graph.AddNode(new Sensor("sensor-02", "binary_sensor.tuyatec_zn9wyqtr_rh3040_6cf5cbfe_ias_zone"));
        var or = new IfAny("kitchen-sensors");
        or.AddInput("sensor-01");
        or.AddInput("sensor-02");
        graph.AddNode(or);

        graph.Connect("sensor-01", "activity", "kitchen-sensors", "sensor-01");
        graph.Connect("sensor-02", "activity", "kitchen-sensors", "sensor-02");
    }

    private static async Task TogglePanasonic(List<EntityState> states, Comms comms)
    {
        string entityID = "light.panasonic_strip";
        string state = states.Find(s => s.entity_id == entityID)?.state ?? throw new Exception();
        
        var callServices = new ClientCallService
        {
            domain = "light",
            service = (state == "on" ? "turn_off" : "turn_on"),
            target = new Attributes
            {
                { "entity_id", entityID }
            }
        };
        await comms.Send(callServices);

        await comms.Receive<ServerMessage>();
    }

    
    private static async Task AllLights(Engine engine)
    {
        var entities = new List<string> { "light.hue_color_spot_1", "light.hue_color_spot_2", "light.hue_color_spot_3", "light.hue_color_spot_4" };
        var color = new List<float> { 0.18325046f, 0.4023088f };
        var callServices = new ClientCallService
        {
            domain = "light",
            service = "turn_on",
            target = new Attributes
            {
                { "entity_id", entities },
            },
            service_data = new Attributes
            {
                { "xy_color", color },
                { "brightness", 50 }
            }
        };
        await engine.comms.Send(callServices);
        await engine.comms.Receive<ServerResult>();
    }

}
