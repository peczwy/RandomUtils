package core.utils;

import core.jobs.IJob;

import java.io.*;

public class SerializatorUtil {

    public static byte[] serialize(IJob job){
        try(ByteArrayOutputStream baos = new ByteArrayOutputStream();
                ObjectOutputStream oos = new ObjectOutputStream(baos)) {
            oos.writeObject(job);
            return baos.toByteArray();
        } catch (IOException e) {
            e.printStackTrace();
            return null;
        }
    }

    public static IJob deserialize(byte[] array){
        try(ByteArrayInputStream bais = new ByteArrayInputStream(array);
                ObjectInputStream ois = new ObjectInputStream(bais)) {
            return (IJob)ois.readObject();
        } catch (IOException | ClassNotFoundException e) {
            e.printStackTrace();
            return null;
        }
    }

}
