/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package amla;

import java.io.IOException;
import java.util.HashMap;
import java.util.logging.Level;
import java.util.logging.Logger;
import javafx.fxml.FXMLLoader;
import javafx.scene.Parent;
import javafx.scene.Scene;

/**
 *
 * @author Kees Kint
 */
public abstract class Switchable {
    private static Scene scene;
    private static final HashMap<String, Switchable> CONTROLLERS = new HashMap<>();
    
    private Parent root;
    
    public static Switchable add(String name){
        Switchable controller = CONTROLLERS.get(name);
        
        if(controller == null){
            try{
                FXMLLoader loader = new FXMLLoader(Switchable.class.getResource(name + ".fxml"));
                Parent root = (Parent) loader.load();
                controller = (Switchable) loader.getController();
                
                controller.setRoot(root);
                
                CONTROLLERS.put(name, controller); 
            }catch(IOException ex){
                Logger.getLogger(Switchable.class.getName()).log(Level.SEVERE, null, ex);
                System.out.println("Error Loading " + name + ".fxml \n\n " + ex);
                controller = null;
            }catch(Exception ex){
                System.out.println("Error Loading " + name + ".fxml \n\n " + ex);
                controller = null;
            }
        }
        return controller;
    }
    
    public static void switchTo(String name){
        Switchable controller = CONTROLLERS.get(name);
        
        if(controller == null){
            controller = add(name);
        }
        if(controller != null){
            if(scene != null){
                scene.setRoot(controller.getRoot());
            }
        }   
    }
    
    public void setRoot(Parent root) {
        this.root = root;
    }
    
    public Parent getRoot(){
        return root;
    }
    
     public static void setScene(Scene scene){
        Switchable.scene = scene;
    }
    
    public Scene getScene(){
        return scene;
    }
    
    public static Switchable getControllerByName(String name){
        return CONTROLLERS.get(name);
    }
}
